using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using System;
using Android.Locations;
using Android.Runtime;
using Android.Content;
using System.Net;
using Newtonsoft.Json;
using static Android.Gms.Maps.GoogleMap;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Support.V4.Content;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Java.IO;
using Android.Provider;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;

namespace App5
{
  
    public class Plane
    {
        public string Icao;
        public double Lat;
        public double Lon;
        public string Mdl;
        public double Trak;
        public double Spd;
        public string Type;
        public string To;
        public string From;
    }

    public static class App
    {
        public static File _file;
        public static File _dir;
        public static Bitmap bitmap;
    }

    [Activity(Label = "Find_my_planesssss", MainLauncher = true)]
    public class MainActivity : Activity, IOnMapReadyCallback,ILocationListener, IOnInfoWindowClickListener
    {
        
        private GoogleMap mMap;
        LocationManager locationManager;
        string provider;
        bool Followplane = false;
        bool Followplane2 = false;
        WebClient webClient = new WebClient();
        //WebClient webClient2 = new WebClient();
        Hashtable planesHash = new Hashtable();
        double lat;
        double lon;
        string url;
        string jsonasd;
        RadioButton Normal;
        RadioButton Follow;
        TextView textview;

        private ImageView _imageView;

        private void RadioButtonClick(object sender, EventArgs e)
        {

            var hi = sender.ToString();
            if (Normal.Checked == true)
            {
                Followplane = false;
                Followplane2 = false;
                Toast.MakeText(this, "You find the planes around you", ToastLength.Short).Show();
                textview.Dispose();

            }
            if (Follow.Checked == true)
            {
                Followplane2 = true;
                //Followplane = true;
                Toast.MakeText(this, "You are following one plane", ToastLength.Short).Show();
            }


        }




        public dynamic Jsonloader(double late, double lng,string icao)
        {

            if (!webClient.IsBusy)
            {
                if (Followplane == false)
                {
                    url = @"https://public-api.adsbexchange.com/VirtualRadar/AircraftList.json?lat=" + late.ToString() + "&lng=" + lng.ToString() + "&fNBnd=" + (late + 0.4).ToString() + "&fSBnd=" + (late - 0.4).ToString() + "&fWBnd=" + (lng - 0.4).ToString() + "&fEBnd=" + (lng + 0.4).ToString();
                }

                else if (Followplane2 == true && Followplane == true)
                {
                    url = @"https://public-api.adsbexchange.com/VirtualRadar/AircraftList.json?fIcoQ=" + jsonasd;
                    System.Console.WriteLine(url);
                }
                else if (Followplane == true)
                {
                    
                }
                url = url.Replace(",", ".");
                var json = webClient.DownloadString(url);
                System.Console.WriteLine(url);
                var planes = JsonConvert.DeserializeObject<dynamic>(json);
                return planes;
            }
            return null;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            mMap = googleMap;
            mMap.SetOnInfoWindowClickListener(this);
     
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
          
           System.Console.WriteLine("thread " + Thread.CurrentThread.ManagedThreadId + "\n");

        }
       
       public void TimerThread()
       {
            while (true)
            {
            
                var planes = Jsonloader(lat, lon, null);

                RunOnUiThread(() => {
                
                    mMap.Clear();
                    planesHash.Clear();
                    foreach (var r in planes["acList"])
                    {
                        Plane plane = new Plane();

                        plane.Lat = (r["Lat"]==null)?0.0:r["Lat"];
                        plane.Lon = (r["Long"] == null) ? 0.0:r["Long"] ;
                        plane.Mdl = (r["Mdl"] == null) ? "":r["Mdl"];
                        plane.Trak = (r["Trak"]== null)?0.0:r["Trak"];
                        plane.Spd = (r["Spd"] == null)?0.0:r["Spd"];
                        plane.Type = (r["Type"] == null)?"":r["Type"];
                        plane.To = (r["To"] == null)?"":r["To"];
                        plane.From = (r["From"] == null)?"":r["From"];
                        plane.Icao = (r["Icao"] == null)?"":r["Icao"];

                        planesHash.Add(plane.Icao, plane);
                        

                        MarkerOptions planemarker = new MarkerOptions();
                        planemarker.SetPosition(new LatLng(plane.Lat, plane.Lon));
                        planemarker.SetTitle(plane.Icao);
                        planemarker.SetRotation((float)plane.Trak);
                        
                        planemarker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pl));
                        //planemarker.SetSnippet("hello");
                       // OnInfoWindowClick(planemarker);

                        mMap.AddMarker(planemarker);
                        
                    }

                    MarkerOptions marker = new MarkerOptions();
                    marker.SetPosition(new LatLng(lat, lon));
                    marker.SetTitle("U zelf");
                    marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.p));

                    mMap.AddMarker(marker);
                });
                Thread.Sleep(5000);
            }                  
       }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == 1)
            {
                System.Console.WriteLine("hi");
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {

            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            if (IsThereAnAppToTakePictures())
            {

                CreateDirectoryForPictures();
                Button button = FindViewById<Button>(Resource.Id.myButton);
                _imageView = FindViewById<ImageView>(Resource.Id.imageView1);
                button.Click += TakeAPicture;

            }

            Normal = FindViewById<RadioButton>(Resource.Id.radioButton1);
            Follow = FindViewById<RadioButton>(Resource.Id.radioButton2);
            textview = FindViewById<TextView>(Resource.Id.textView1);

            Normal.Click += RadioButtonClick;
            Follow.Click += RadioButtonClick;
            

            SetUpMap();

                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.AccessFineLocation }, 1);

                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
                {
                    locationManager = (LocationManager)GetSystemService(Context.LocationService);
                    provider = locationManager.GetBestProvider(new Criteria(), false);
                    Location location = locationManager.GetLastKnownLocation(provider);

                    if (location != null)
                    {
                        lat = location.Latitude;//location.Latitude;
                        lon = location.Longitude;//location.Longitude;
                    }
                    else
                    {
                        lat = 51.9204144;//location.Latitude;
                        lon = 4.4840513;//location.Longitude;

                        AlertDialog.Builder alertDialog = new AlertDialog.Builder(this);
                        alertDialog.SetTitle("WARNING");
                        alertDialog.SetMessage("Cant find a location");
                        alertDialog.SetNeutralButton("OK", delegate { alertDialog.Dispose(); });
                        
                        alertDialog.Show();
                    }

                    Thread timerThread = new Thread(new ThreadStart(this.TimerThread));
                    timerThread.Start();

                }
                else
                {
                    System.Console.WriteLine("dikke moeder");
                }
            
        }
        
        private void SetUpMap()
        {
            if (mMap == null)
            {
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.map).GetMapAsync(this);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == (int)Permission.Granted)
            {
                
                    locationManager.RequestLocationUpdates(provider, 400, 1, this);

            }
            
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == (int)Permission.Granted)
            {
                
                    locationManager.RemoveUpdates(this);
               
            }
            }

        public void OnLocationChanged(Location location)
        {
            
                lat = location.Latitude;
                lon = location.Longitude;
            
        }
  
        public void OnProviderDisabled(string provider)
        {
            //throw new NotImplementedException();
        }

        public void OnProviderEnabled(string provider)
        {
            throw new NotImplementedException();
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            //throw new NotImplementedException();
        }

        public void OnInfoWindowClick(Marker marker)
        {
            LatLng pos = marker.Position;
            mMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(pos, 10));

            jsonasd = marker.Title;
            string icao = marker.Title;
           
            Plane plane = (Plane)planesHash[icao];
           

            AlertDialog.Builder alertDialog = new AlertDialog.Builder(this);
             alertDialog.SetTitle(plane.Icao);
             alertDialog.SetMessage("Model of plane " + plane.Mdl + "\n" + "Speed of plane: " + plane.Spd+ "\n" + 
                                   "Type of plane: "+ plane.Type + "\n" + 
                                   "From airport: "+ plane.From + "\n" +
                                   "Towards airport: "+ plane.To + "\n");
             alertDialog.SetNeutralButton("OK", delegate { alertDialog.Dispose(); });
            alertDialog.SetNegativeButton("Follow", delegate { alertDialog.Dispose(); Followplane = true; /*Jsonloader(plane.Lon, plane.Lat, icao);*/ });
            alertDialog.Show();
            
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {

            base.OnActivityResult(requestCode, resultCode, data);

            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);

            Uri contentUri = Uri.FromFile(App._file);

            mediaScanIntent.SetData(contentUri);
            //hahasol
            SendBroadcast(mediaScanIntent);

            int height = Resources.DisplayMetrics.HeightPixels;

            int width = _imageView.Height;

            App.bitmap = App._file.Path.LoadAndResizeBitmap(width, height);

            if (App.bitmap != null)
            {

                _imageView.SetImageBitmap(App.bitmap);

                App.bitmap = null;

            }



            // Dispose of the Java side bitmap.

            GC.Collect();

        }

        
        private void CreateDirectoryForPictures()

        {

            App._dir = new File(

                Environment.GetExternalStoragePublicDirectory(

                    Environment.DirectoryPictures), "planes");

            if (!App._dir.Exists())

            {

                App._dir.Mkdirs();

            }

        }



        private bool IsThereAnAppToTakePictures()

        {

            Intent intent = new Intent(MediaStore.ActionImageCapture);

            IList<ResolveInfo> availableActivities =

                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);

            return availableActivities != null && availableActivities.Count > 0;

        }



        private void TakeAPicture(object sender, EventArgs eventArgs)

        {

            Intent intent = new Intent(MediaStore.ActionImageCapture);



            App._file = new File(App._dir, String.Format("plane_{0}.jpg", Guid.NewGuid()));



            intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(App._file));



            StartActivityForResult(intent, 0);

        }

    }
}


