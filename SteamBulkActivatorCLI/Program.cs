/*******
 * CLI Steam Key Activator.
 * Adapted from SteamBulkActivator by Ezzpify
 * https://github.com/Ezzpify/SteamBulkActivator
 * by Luke Tomkus https://github.com/pkillboredom
 * *****/

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CommandLine;
using Steam4NET;
using System.ComponentModel;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SteamBulkActivatorCLI
{
    class Options
    {
        [Option("path", Required = true, HelpText = "Path to key list.")]
        public string path { get; set; }

        [Option("logpath", Required = false, HelpText = "Optional path to create a log directory.")]
        public string logpath { get; set; }

        [Option("danger", Required = false, HelpText = "Ignore risk of being rate-limited on steam.")]
        public bool danger { get; set; }
    }


    class Program
    {
        private const int REGISTER_DELAY = 1000;
        private static string txtKeys = "";
        private static bool _inRegistration = true;

        private static int _user, _pipe;
        private static int _registerDelay;
        private static bool _waitingForActivationResp = false;

        private static ISteam006 _steam006;
        private static IClientUser _clientUser;
        private static IClientEngine _clientEngine;
        private static IClientBilling _clientBilling;
        private static ISteamClient012 _steamClient012;
        private static ResultObject _result;
        private static List<string> _cdKeyList;
        private static BackgroundWorker _callbackBwg;
        private static BackgroundWorker _purchaseBwg;

        static void Main(string[] args)
        {
            string keysPath = null;
            string logPath = null;
            bool dangerMode = false;
            _cdKeyList = new List<string>();

            // Parse Args
            var result = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    keysPath = options.path;
                    logPath = options.logpath;
                    dangerMode = options.danger;
                })
                .WithNotParsed(errors =>
                {
                    foreach (Error err in errors)
                    {
                        Console.WriteLine(err);
                    }
                    System.Environment.Exit(160);
                });

            if (logPath != null && logPath != "") 
            {
                FileStream ostrm;
                StreamWriter writer;
                TextWriter oldOut = Console.Out;
                try
                {
                    ostrm = new FileStream(logPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt", FileMode.OpenOrCreate, FileAccess.Write);
                    writer = new StreamWriter(ostrm);
                    Console.SetOut(writer);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cannot open log file for writing");
                    Console.WriteLine(e.Message);
                    return;
                }
            }

            // Attempt to load key file.
            StreamReader sr;
            try
            {
                sr = new StreamReader(keysPath, Encoding.UTF8);
                txtKeys = sr.ReadToEnd();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                System.Environment.Exit(0);
            }

            addKeysToList();

            if(_cdKeyList.Count > 30 && !dangerMode)
            {
                Console.WriteLine("Your key file had {0} lines. Attempting to activate over 30 keys at once risks rate-limiting.\nEnable --danger to bypass.", _cdKeyList.Count);
                System.Environment.Exit(0);
            }
            
            if(_cdKeyList.Count <= 0)
            {
                Console.WriteLine("There were no valid keys found.");
                System.Environment.Exit(0);
            }

            // Check Steam is running.
            if(ConnectToSteam())
            {
                if (!_clientUser.BLoggedOn())
                {
                    Console.WriteLine("Fatal: Not Logged into steam.");
                    Environment.Exit(0);
                }
                _callbackBwg = new BackgroundWorker() { WorkerSupportsCancellation = true };
                _callbackBwg.DoWork += _callbacks_DoWork;
                _callbackBwg.RunWorkerCompleted += _callbacks_RunWorkerCompleted;

                _purchaseBwg = new BackgroundWorker() { WorkerSupportsCancellation = true };
                _purchaseBwg.DoWork += _purchaseBwg_DoWork;

                registerKeys();

                while (!_result.Completed)
                    Thread.Sleep(250);

                Console.WriteLine(_result.GetResults());

                Environment.Exit(0);
            }
        }

        private static bool ConnectToSteam()
        {
            var steamError = new TSteamError();
            string errorText = "";

            if (!Steamworks.Load(true))
            {
                errorText = "Steamworks failed to load.";
                Console.WriteLine(errorText);
                return false;
            }

            _steam006 = Steamworks.CreateSteamInterface<ISteam006>();
            if (_steam006.Startup(0, ref steamError) == 0)
            {
                errorText = "Steam startup failed. Is Steam running?";
                Console.WriteLine(errorText);
                return false;
            }

            _steamClient012 = Steamworks.CreateInterface<ISteamClient012>();
            _clientEngine = Steamworks.CreateInterface<IClientEngine>();

            _pipe = _steamClient012.CreateSteamPipe();
            if (_pipe == 0)
            {
                errorText = "Failed to create user pipe.";
                Console.WriteLine(errorText);
                return false;
            }

            _user = _steamClient012.ConnectToGlobalUser(_pipe);
            if (_user == 0 || _user == -1)
            {
                errorText = "Failed to connect to global user.";
                Console.WriteLine(errorText);
                return false;
            }

            _clientBilling = _clientEngine.GetIClientBilling<IClientBilling>(_user, _pipe);
            _clientUser = _clientEngine.GetIClientUser<IClientUser>(_user, _pipe);
            return true;
        }
        private static void _purchaseBwg_DoWork(object sender, DoWorkEventArgs e)
        {
            //while (_result != null)
            //{
            //    Thread.Sleep(100);
            //}

            var cdkeys = (List<string>)e.Argument;
            for (int i = 0; i < cdkeys.Count; i++)
            {
                if (_purchaseBwg.CancellationPending)
                    break;

                _waitingForActivationResp = true;
                string pchActivationCode = cdkeys[i];
                _clientBilling.PurchaseWithActivationCode(pchActivationCode);

                if (i + 1 < cdkeys.Count)
                {
                    if (_registerDelay != 0)
                        Thread.Sleep(_registerDelay);
                }

                while (_waitingForActivationResp)
                    Thread.Sleep(50);
            }

            completedRegistration();
        }

        private static void _callbacks_DoWork(object sender, DoWorkEventArgs e)
        {
            CallbackMsg_t callbackMsg = new CallbackMsg_t();
            while (!_callbackBwg.CancellationPending)
            {
                while (Steamworks.GetCallback(_pipe, ref callbackMsg) && !_callbackBwg.CancellationPending)
                {
                    switch (callbackMsg.m_iCallback)
                    {
                        case PurchaseResponse_t.k_iCallback:
                            onPurchaseResponse((PurchaseResponse_t)Marshal.PtrToStructure(callbackMsg.m_pubParam, typeof(PurchaseResponse_t)));
                            break;
                    }

                    Steamworks.FreeLastCallback(_pipe);
                }

                Thread.Sleep(100);
            }
        }

        private static void _callbacks_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Console.WriteLine($"Uhhh...\n\n{e.Error}", "Callback error");
            }
        }

        private static void onPurchaseResponse(PurchaseResponse_t callback)
        {
            EPurchaseResultDetail result = (EPurchaseResultDetail)callback.m_EPurchaseResultDetail;
            switch (result)
            {
                case EPurchaseResultDetail.k_EPurchaseResultTooManyActivationAttempts:
                    _purchaseBwg.CancelAsync();
                    completedRegistration();
                    break;
            }

            _result.AddResult(Utils.GetFriendlyEPurchaseResultDetailMsg(result));
            _waitingForActivationResp = false;
        }

        private static void registerKeys()
        {
            _registerDelay = REGISTER_DELAY;

            _purchaseBwg.RunWorkerAsync(_cdKeyList);
            _callbackBwg.RunWorkerAsync();

            _result = new ResultObject(_cdKeyList, _registerDelay);
        }

        private static void completedRegistration()
        {
            _callbackBwg.CancelAsync();
            _result.Completed = true;
        }

        private static void addKeysToList(bool regexCheck = true)
        {
            var tempList = new List<string>();
            string cdKeyPattern = @"([A-Za-z0-9]+)(-([A-Za-z0-9]+)){2,}";
            foreach (Match m in Regex.Matches(txtKeys, cdKeyPattern))
            {
                if (tempList.Contains(m.Value))
                    continue;

                tempList.Add(m.Value);
            }
            _cdKeyList = tempList;
            Console.WriteLine($"Valid keys: {_cdKeyList.Count}");
        }
    }
}
