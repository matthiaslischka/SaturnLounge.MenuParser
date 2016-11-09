using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Path = System.IO.Path;

namespace SaturnLounge.MenuParser
{
    internal class MenuParser
    {
        private const string DownloadPath = @"c:\tmp\saturn-lounge-menus";
        private const string DownloadUrl = @"http://www.saturn-lounge.at/wp-content/uploads/menu.pdf";


        public static void Main()
        {
            var menuFile = EnsureMenuFile();

            var menuText = ExtractTextFromPdf(menuFile);

            var todaysMenu = GetTodaysMenu(menuText);

            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine(PrettyPrintText(todaysMenu));

            Console.ReadLine();
        }

        private static FileInfo EnsureMenuFile()
        {
            Directory.CreateDirectory(DownloadPath);

            var currentCulture = CultureInfo.CurrentCulture;
            var weekNr = currentCulture.Calendar.GetWeekOfYear(
                DateTime.Today,
                currentCulture.DateTimeFormat.CalendarWeekRule,
                currentCulture.DateTimeFormat.FirstDayOfWeek);

            var fileName = Path.Combine(DownloadPath, $"{weekNr}.pdf");

            if (!File.Exists(fileName))
                using (var client = new WebClient())
                {
                    client.DownloadFile(DownloadUrl, fileName);
                }

            return new FileInfo(fileName);
        }

        private static string ExtractTextFromPdf(FileInfo menuFile)
        {
            using (var reader = new PdfReader(menuFile.FullName))
            {
                var text = new StringBuilder();

                for (var i = 1; i <= reader.NumberOfPages; i++)
                    text.Append(PdfTextExtractor.GetTextFromPage(reader, i));

                return text.ToString();
            }
        }

        private static string GetTodaysMenu(string fullMenu)
        {
            var weekDays = new[]
{
                "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag",
                "Sonntag"
            };

            var menuSplittedByDays = fullMenu.Split(weekDays, StringSplitOptions.RemoveEmptyEntries);

            return menuSplittedByDays.FirstOrDefault(s => s.Contains(DateTime.Today.ToShortDateString()));
        }

        private static string PrettyPrintText(string text)
        {
            var textWithoutMultipleEmptyRows = Regex.Replace(text, @"^(\s*\n){2,}|^(\s*\r\n){2,}", "", RegexOptions.Multiline);
            return textWithoutMultipleEmptyRows.Replace(", ", "");
        }
    }
}