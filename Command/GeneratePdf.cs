using DataService.Entity;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentSignalData.Command
{
    internal class GeneratePdf
    {
        public void GeneratePdfReport(List<Valve> valves)
        {

            PdfDocument document = new PdfDocument();
            document.Info.Title = "Valve Report";


            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont titleFont = new XFont("Verdana", 20);
            XFont regularFont = new XFont("Verdana", 12);

            gfx.DrawString("Valve Report", titleFont, XBrushes.Black, new XRect(0, 0, page.Width, 50), XStringFormats.TopCenter);


            gfx.DrawString($"Generated on: {DateTime.Now}", regularFont, XBrushes.Black, new XRect(20, 60, page.Width, page.Height), XStringFormats.TopLeft);

            int yPoint = 100;
            foreach (var valve in valves)
            {
                string valveInfo = $"Valve: {valve.Name}, Open Count: {valve.OpenCount}";
                gfx.DrawString(valveInfo, regularFont, XBrushes.Black, new XRect(20, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                yPoint += 20;

                if (yPoint > page.Height - 50)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPoint = 50; 
                }
            }

            string filename = "ValveReport.pdf";
            document.Save(filename);

            Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
        }
    }
}
