using System;
using System.Collections.Generic;
using System.Text;

namespace ProjektOI
{
    public static class Schedulecompere1
    {
        public static List<(DateTime Poczatek, DateTime Koniec)> 

            FindCommonFreeTime(List<TimetableEntry> planA, List<TimetableEntry> planB, DateOnly data)
        {
            DateTime poczD = data.ToDateTime(new TimeOnly(8, 0));
            //DateTime konD = data.ToDateTime(new TimeOnly(20, 0));

            // zajecia 8-20 każda godzina dziel na kwadranse 12*4 = 48
            bool[] zajetykwadrans = new bool[48];
            // false wolni, true a lub b ma zajecia 
            DateTime poczatekDnia = data.ToDateTime(new TimeOnly(8, 0));

            // DateTime pocatekDnia = ;
            foreach (TimetableEntry t in planA)
            {
                if(DateOnly.FromDateTime(t.StartTime)==data)
                {
                    NaZajeciach(zajetykwadrans, poczD, lesson.SatrtTime, lesson.EndTime);

                }
            }
            foreach (TimetableEntry t in planB)
            {
                if (DateOnly.FromDateTime(t.StartTime) == data)
                {
                    NaZajeciach(zajetykwadrans, poczD, lesson.SatrtTime, lesson.EndTime);

                }
            }

            //odczyt kiedy osoba ma wolne przedzialy 
            List<(DateTime  Poczatek, DateTime Koniec)> wolneKwadr = new List<(DateTime, DateTime)>();
            DateTime? freeStart = null;

            for (int i = 0;i< zajetykwadrans.Length;i++)
            {
                DateTime kwadransaktualny = poczD.AddMinutes(i * 15);

                //jesli wolna jesli zajeta

                if (zajetykwadrans[i] == false)
                {
                    if (freeStart == null)
                    {
                        freeStart  = kwadransaktualny;
                    }
                }
                else
                {

                    if (freeStart != null)
                    {
                        freeStart = wolneKwadr.Add(freeStart.Value, kwadransaktualny);
                        freeStart = null;
                    }
                }
            }

            if(freeStart!= null)
            {
                wolneKwadr.Add((freeStart.Valure, poczatekDnia.AddHours(12) ));
            }
            //zamkniecie wolnego okienka
            //j
            


            return wolneKwadr;
        }

        private static void NaZajeciach(bool[] zajetykwadrans, DateTime poczD, DateTime zajeciaPocz, DateTime zajeciaKon)
        {
            for(int i = 0;i< zajetykwadrans.Length;i++ )
            {
                DateTime poczkwadransu = poczD.AddMinutes(i * 15);
                DateTime  konkwadransu = poczkwadransu.AddMinutes(15);

                if(zajeciaPocz< konkwadransu && zajeciaKon > poczkwadransu)
                {
                    zajetykwadrans[i] = true;
                }


            }
        }
    }
}
