using System;
using Android.App;
using Android.Widget;
using Android.OS;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Java.Net;
using System.Security.Policy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Android.Provider.ContactsContract;

namespace WebService
{
    [Activity(Label = "WebService", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        TextView txtTarih;
        Button btnBitis;
        Button btnBilgileriAl;
        public ExchangeRatesCrawlerPostData Data { get; set; } = new ExchangeRatesCrawlerPostData();
        ListView items;
        List<string> mItems;
        List<string> jsonTitle;
        Spinner spnrTitle;
        

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            txtTarih = FindViewById<TextView>(Resource.Id.txtTarih);
            items = FindViewById<ListView>(Resource.Id.listJson);
            btnBitis = FindViewById<Button>(Resource.Id.btnBitisTarih);
            btnBilgileriAl = FindViewById<Button>(Resource.Id.btnGet);
            spnrTitle = FindViewById<Spinner>(Resource.Id.spnrTitle);
          
           mItems = new List<string>();
           
            btnBitis.Click += (sender, e) => ShowDatePicker(time =>
                txtTarih.Text = (Data.BeginDate = Data.EndDate = time).ToString("yyyy-MM-ddThh:mm:ss.fffZ")
            );
            btnBilgileriAl.Click += async (sender, e) =>
            {
                Toast.MakeText(this, "Paket Verileri Yükleniyor", ToastLength.Long).Show();
                string url = "http://peakupexchangerates.azurewebsites.net/api/Crawler";

                JArray json = await FetchAsync(url);
                ParseAndDisplay(json);

            };
        }
        private async Task<JArray> FetchAsync(string url)
        {
            string value = JsonConvert.SerializeObject(Data);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.ContentLength = value.Length;

            request.Method = "POST";

            using (Stream requestStream = request.GetRequestStream())
            {
                using (StreamWriter writer = new StreamWriter(requestStream))
                {
                    writer.Write(value);
                }
            }
            using (WebResponse response = await request.GetResponseAsync())
            {
                // Get a stream representation of the HTTP web response:
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                        return JsonConvert.DeserializeObject<JArray>(reader.ReadToEnd());
                }
            }
        }

        public void ParseAndDisplay(JArray json)
        {
            string[] title = {"Date : ", "Currency Code : ","Unit : ","Currency Name : ","Forex Buying : ","Forex Selling : ","Baknote Buying : ","Baknote Selling : "};

            try
            {  
                 jsonTitle = new List<string>();
                for (int i = 0; i < json.Count; i++)
                {
                    jsonTitle.Add(json[i][3].ToString());
                    ArrayAdapter<string> adapterTitle = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, jsonTitle);
                    spnrTitle.Adapter = adapterTitle;
                }

                 spnrTitle.ItemSelected += (sender, e) =>
                 {
                     mItems.Clear();
                   
                     string selectedTitle = spnrTitle.SelectedItem.ToString();
                     for (int i = 1; i < json.Count; i++)
                     {
                         if (selectedTitle == json[i][3].ToString())
                         {
                             for (int j = 0; j < title.Length; j++)
                             {
                                 mItems.Add(title[j] + json[i][j].ToString());
                             }
                         }
                         
                     }

                     ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mItems);
                     items.Adapter = adapter;
                 };
            }
            catch (Exception e)
            {
                Toast.MakeText(this, e.ToString(), ToastLength.Long).Show();
            }
        }
        private void ShowDatePicker(Action<DateTime> act)
        {
            DatePickerFragment frag = DatePickerFragment.NewInstance(act);
            frag.Show(FragmentManager, DatePickerFragment.TAG);
        }
    }

    public class ExchangeRatesCrawlerPostData
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

