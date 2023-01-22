using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace parser

{
    class Program

    {
        static void Main(string[] args)
            
            {
                var driver = WebDriverManager();
                driver.Navigate().GoToUrl("https://www.rbc.ru/");
                string[] links = driver.FindElements(By.ClassName("main__feed__link")).Select(x => x.GetAttribute("href")).ToArray();
                
                var data = Parser(links, driver);
                WriteToJSON(data);
                WriteToCSV(data);
                WriteToXML(data);
                driver.Quit();
                Console.WriteLine("Done!");
                
            }

        private static IWebDriver WebDriverManager()
        {
            var option = new EdgeOptions();
            option.AddArgument("--headless");
            option.AddArgument("--disable-gpu");
            option.AddArgument("--no-sandbox");
            var path = @"C:\Users\Administrator\Documents\parser_rbc_cs";
            // msedgedriver.exe
            IWebDriver driver = new EdgeDriver(path, option);

            return driver;
        }

        private static Dictionary<string, Dictionary<string, string>> Parser(string[] links, IWebDriver driver)
        {
            var data = new Dictionary<string, Dictionary<string, string>>();

            foreach (var link in links)
            {
                driver.Navigate().GoToUrl(link);

                string title;
                string category;
                string time;
                string text;
                string[] authors;
                string[] tags;
                string[] persons;

                try{
                    title = driver.FindElement(By.ClassName("article__header__title-in")).Text;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                };
                try{
                    category = driver.FindElement(By.ClassName("article__header__category")).Text;
                }
                catch (Exception e)
                {
                    category = " ";
                    Console.WriteLine(e.Message);
                };
                try{
                    time = driver.FindElement(By.ClassName("article__header__date")).Text;
                }
                catch (Exception e)
                {
                    time = " ";
                    Console.WriteLine(e.Message);
                };
                try{
                    text = driver.FindElement(By.ClassName("article__text__overview")).Text;
                }
                catch (Exception e)
                {
                    text = " ";
                    Console.WriteLine(e.Message);
                };
                try{
                    authors = driver.FindElements(By.ClassName("article__authors__author__name")).Select(x => x.Text).ToArray();
                }
                catch (Exception e)
                {
                    authors = new string[0];
                    Console.WriteLine(e.Message);
                };
                try{
                    tags = driver.FindElements(By.ClassName("article__tags__item")).Select(x => x.Text).ToArray();
                }
                catch (Exception e)
                {
                    tags = new string[0];
                    Console.WriteLine(e.Message);
                };
                try{
                    persons = driver.FindElements(By.ClassName("person-card__title")).Select(x => x.Text).ToArray();
                }
                catch (Exception e)
                {
                    persons = new string[0];
                    Console.WriteLine(e.Message);
                };

                data.Add(title, new Dictionary<string, string>()
                {
                    { "category", category },
                    { "time", time },
                    { "text", text },
                    { "authors", string.Join(", ", authors) },
                    { "tags", string.Join(", ", tags) },
                    { "persons", string.Join(", ", persons) }
                });
            }

            return data;

        }
        private static void WriteToJSON(Dictionary<string, Dictionary<string, string>> data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText("data.json", json);
        }
        private static void WriteToCSV(Dictionary<string, Dictionary<string, string>> data)
        {
            var csv = new StringBuilder();
            var header = string.Join(",", data.First().Value.Keys);
            csv.AppendLine(header);
            foreach (var item in data)
            {
                var values = string.Join(",", item.Value.Values);
                csv.AppendLine(values);
            }
            File.WriteAllText("data.csv", csv.ToString());
        }
        private static void WriteToXML(Dictionary<string, Dictionary<string, string>> data)
        {
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<data>");
            foreach (var item in data)
            {
                xml.AppendLine($"<item title=\"{item.Key}\">");
                foreach (var value in item.Value)
                {
                    xml.AppendLine($"<{value.Key}>{value.Value}</{value.Key}>");
                }
                xml.AppendLine("</item>");
            }
            xml.AppendLine("</data>");
            File.WriteAllText("data.xml", xml.ToString());
        }
    }
}
