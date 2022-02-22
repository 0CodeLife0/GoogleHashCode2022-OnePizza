using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace OnePizza0
{
    class Program
    {
        static string path = @"put\";
        static List<ingredient_info> myingredient_info_list = new List<ingredient_info>();
        static List<ing_all> mying_all_list = new List<ing_all>();
        // static string filename = "a_an_example.in.txt";
        static public List<string> filenamelist = new List<string>();
        static void Main(string[] args)
        {
            // un comment each file to run it
            filenamelist.Add("a_an_example.in.txt");
            filenamelist.Add("b_basic.in.txt");
            filenamelist.Add("c_coarse.in.txt");
            // filenamelist.Add("d_difficult.in.txt");
            // filenamelist.Add("e_elaborate.in.txt");

            foreach (var myfilename in filenamelist)
            {
                myingredient_info_list = new List<ingredient_info>();
                mying_all_list = new List<ing_all>();
                
                picky_client picky_clients = new picky_client();
                Console.WriteLine("- Start Reading - " + myfilename);
                picky_clients = ReadMyInput(path + myfilename);

                Pizza mypizza = CreatePizza(picky_clients);

                WriteOutput(mypizza, path + myfilename);

                Console.WriteLine("N of selected ing= " + mypizza.n_ingredients + "\n- Finished Running - " + myfilename);
            }
        }

        static Pizza CreatePizza(picky_client mypicky_clients)
        {
            Pizza mypizza = new Pizza();
            Console.WriteLine("# Loved =" + myingredient_info_list.Where(p => p.selected == true).Count() + " - # Hated = " + myingredient_info_list.Where(p => p.selected == false).Count());
            mypizza.ingredient_name_list.AddRange(myingredient_info_list.Where(p => p.selected == true).Select(p => p.ingredient_name).ToList());
            mypizza.n_ingredients = mypizza.ingredient_name_list.Count();
            Console.WriteLine("Pizza Done!");
            return mypizza;
        }

        static picky_client ReadMyInput(string full_path)
        {
            picky_client mypicky_client = new picky_client();

            var MyInputFile = File.ReadAllLines(full_path);
            mypicky_client.n_potential_clients = int.Parse(MyInputFile[0]);

            for (int i = 1; i <= mypicky_client.n_potential_clients * 2; i += 2)
            {
                var loved_ing_line = MyInputFile[i].Split(' ');
                var hated_ing_line = MyInputFile[i + 1].Split(' ');
                // when running elaborate, if you filter out clients who hate more then 3 ings you get a better score
                // Thanks to: https://youtu.be/AQnUJgt6tb0
                // but that's not the case for the difficut file
                if (!full_path.Contains("elaborate") ||
                (full_path.Contains("elaborate")&&int.Parse(hated_ing_line[0]) < 3))
                {
                    client_preference myclient_Preference = new client_preference();

                    myclient_Preference.n_loved_ingredients = int.Parse(loved_ing_line[0]);
                    for (int j = 1; j <= myclient_Preference.n_loved_ingredients; j++)
                    {
                        myclient_Preference.loved_ingredients.Add(loved_ing_line[j]);
                        mying_all_list.Add(new ing_all(i, loved_ing_line[j], true));
                    }

                    myclient_Preference.n_hated_ingredients = int.Parse(hated_ing_line[0]);
                    for (int j = 1; j <= myclient_Preference.n_hated_ingredients; j++)
                    {
                        myclient_Preference.hated_ingredients.Add(hated_ing_line[j]);
                        mying_all_list.Add(new ing_all(i, hated_ing_line[j], false));
                    }
                    mypicky_client.client_preferences.Add(myclient_Preference);
                    Console.WriteLine(((double)mypicky_client.client_preferences.Count() / (double)mypicky_client.n_potential_clients) * 100 + "% read");
                }
            }

            var ing_list = (from ing_info in mying_all_list.ToList()
                            select new
                            {
                                ing_info.ing_name,
                                n_loved = mying_all_list.Where(p => p.ing_name == ing_info.ing_name && p.is_loved == true).Count(),
                                n_hated = mying_all_list.Where(p => p.ing_name == ing_info.ing_name && p.is_loved == false).Count(),

                            }).Distinct().ToList();

            foreach (var item in ing_list)
            {
                if (myingredient_info_list.Where(p => p.ingredient_name == item.ing_name).Count() == 0)
                    myingredient_info_list.Add(new ingredient_info(item.ing_name, item.n_loved, item.n_hated));
            }
            return mypicky_client;
        }
        static void WriteOutput(Pizza mypizza, string full_path)
        {
            string createText = mypizza.n_ingredients.ToString();

            foreach (var ing in mypizza.ingredient_name_list)
            {
                createText += " " + ing;
            }

            File.WriteAllText(full_path + "_output.txt", createText);
        }

    }
    //Input File classes
    class picky_client
    {
        public picky_client()
        {
        }
        public int n_potential_clients { get; set; }
        public List<client_preference> client_preferences { get; set; } = new List<client_preference>();
    }
    class client_preference
    {
        public int n_loved_ingredients { get; set; }
        public List<string> loved_ingredients { get; set; } = new List<string>();
        public int n_hated_ingredients { get; set; }
        public List<string> hated_ingredients { get; set; } = new List<string>();
    }

    class ingredient_info
    {
        public ingredient_info(string name, int n_loved, int n_hated)
        {
            this.n_loved = n_loved;
            this.n_hated = n_hated;
            double loved_to_hate = n_hated == 0 ? 10 : (double)n_loved / (double)n_hated;
            selected = loved_to_hate >= 1;
            // selected = n_hated<=1&&n_loved>0&&();
            ingredient_name = name;
        }
        public string ingredient_name { get; set; }
        public int n_loved { get; set; }
        public int n_hated { get; set; }
        public bool selected { get; set; }
    }

    class ing_all
    {
        public ing_all(int client_id, string ing_name, bool is_loved)
        {
            this.client_id = client_id;
            this.ing_name = ing_name;
            this.is_loved = is_loved;
        }
        public bool is_loved { get; set; }
        public string ing_name { get; set; }
        public int client_id { get; set; }
    }
    // Output File class
    class Pizza
    {
        public int n_ingredients { get; set; }
        public List<string> ingredient_name_list { get; set; } = new List<string>();
    }
}
