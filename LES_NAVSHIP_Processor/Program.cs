using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LES_NAVSHIP_Routine;

namespace LES_NAVSHIP_Processor
{
    internal class Program
    {
        private static System.Globalization.CultureInfo _defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
        static void Main(string[] args)
        {
            LES_NAVSHIP_Routine.NAVSHIP_Routine routine = new LES_NAVSHIP_Routine.NAVSHIP_Routine();

            try
            {
                string processor_name = Convert.ToString(ConfigurationManager.AppSettings["PROCESSOR_NAME"]) + " : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                if (LeSSysInfo.LeSMain.GetLeSSysInfo() == LeSSysInfo.LeSReturn.Success_1)
                {
                    LES_NAVSHIP_Routine.NAVSHIP_Routine obj = new LES_NAVSHIP_Routine.NAVSHIP_Routine();
                    SetCulture(obj);
                    obj.LogText = "====================================";
                    obj.LogText = processor_name + "  process started...";
                    obj.loadAppsettings();
                    obj.LogText = processor_name + "  process completed...";
                    obj.LogText = "====================================";
                    Console.ReadLine();
                }

            }
            catch (Exception ex)
            {
                routine.LogText = "Error while running LeS_NavShip_Processor : " + ex.Message;
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        public static void SetCulture(LES_NAVSHIP_Routine.NAVSHIP_Routine _oRoutine)
        {
            System.Globalization.CultureInfo _defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            _oRoutine.LogText = "Default Regional Setting - " + _defaultCulture.DisplayName;
            _oRoutine.LogText = "Current Regional Setting - " + System.Threading.Thread.CurrentThread.CurrentCulture.DisplayName;
            Console.Title = Convert.ToString(ConfigurationManager.AppSettings["PROCESSOR_NAME"]) + " : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
