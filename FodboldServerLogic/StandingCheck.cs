using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using FodboldApp.Model;
using Realms;
using Realms.Sync;

namespace FodboldServerLogic
{
    //uploads standings list to the ROS
    class StandingCheck
    {
        static string fileName = "standing.xml";
        static string League = "2. Division";
        static string ClubName = "Frem";
        static Realm _realm;

        static void Main2() {
            var user = User.LoginAsync(Credentials.UsernamePassword("realm-admin", "bachelor", false), new Uri($"http://13.59.205.12:9080")).Result;
            SyncConfiguration config = new SyncConfiguration(user, new Uri($"realm://13.59.205.12:9080/data/standings"));
            _realm = Realm.GetInstance(config);
            _realm.Write(() =>
            {
                _realm.RemoveAll();
            });
            XDocument doc = XDocument.Load(fileName);

            foreach (XElement el in doc.Root.Elements())
            {
                if (el.Attribute("league").Value.Contains(League))
                {
                    foreach (XElement LeagueMember in el.Elements())
                    {
                        LeagueTableModel table = new LeagueTableModel();
                        table.Position = LeagueMember.Attribute("rank").Value;
                        table.Points = LeagueMember.Attribute("points").Value;
                        table.GoalsAgainst = LeagueMember.Attribute("goalsagainst").Value;
                        table.GoalsFor = LeagueMember.Attribute("goalsfor").Value;
                        table.Losses = LeagueMember.Attribute("defeits").Value;
                        table.Draws = LeagueMember.Attribute("draws").Value;
                        table.Wins = LeagueMember.Attribute("wins").Value;
                        table.MP = LeagueMember.Attribute("played").Value;
                        table.Team = LeagueMember.Attribute("participantname").Value;
                        table.GroupName = el.Attribute("league").Value;
                        _realm.Write(() =>
                        {
                            _realm.Add(table);
                        });
                    }
                }
            }
            Console.WriteLine("done");
            Thread.Sleep(10000);
        }
                               
    }
}
