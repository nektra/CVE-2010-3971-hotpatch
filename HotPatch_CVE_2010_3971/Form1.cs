using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Nektra.Deviare2;
using System.IO.Pipes;


namespace DeviareTest
{
    public partial class Form1 : Form
    {
        private NktSpyMgr _spyMgr;
        private NktProcess _process;
        private IntPtr _RVA;


        public Form1()
        {
            InitializeComponent();

            _spyMgr = new NktSpyMgr();
            _spyMgr.Initialize();
            _spyMgr.OnFunctionCalled += new DNktSpyMgrEvents_OnFunctionCalledEventHandler(OnFunctionCalled);

            LoadSymbolTable();
        }

        private void LoadSymbolTable()
        {
            NktTools _tools = new NktTools();
           
            NktPdbFunctionSymbol pdbSym = _tools.LocateFunctionSymbolInPdb(@"C:\Windows\System32\mshtml.dll",
                @"CStyleSheet::Notify",
                @"http://msdl.microsoft.com/download/symbols",
                @"D:\PDB");
             

            if (pdbSym != null)
            {
                _RVA = pdbSym.AddrOffset;
            }
        }

        private bool HookProcess(string proccessName)
        {
            NktProcessesEnum enumProcess = _spyMgr.Processes();
            NktProcess tempProcess = enumProcess.First();
            while (tempProcess != null)
            {
                if (tempProcess.Name.Equals(proccessName, StringComparison.InvariantCultureIgnoreCase) && tempProcess.PlatformBits > 0 && tempProcess.PlatformBits <= IntPtr.Size * 8)
                {
                    _process = tempProcess;

                    NktModule module = _process.ModuleByName("mshtml.dll");

                    if (module != null)
                    {
                        IntPtr EA = (IntPtr)new IntPtr(module.BaseAddress.ToInt32() + _RVA.ToInt32());

                        NktHook hook = _spyMgr.CreateHookForAddress(EA, "mshtml.dll!CStyleSheet::Notify", (int)(eNktHookFlags.flgRestrictAutoHookToSameExecutable | eNktHookFlags.flgOnlyPreCall | eNktHookFlags.flgDontCheckAddress));

                        hook.Attach(_process, true);
                        hook.Hook(true);
                    }

                }
                tempProcess = enumProcess.Next();
            }

            _process = null;
            return false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            HookProcess("iexplore.exe");

           
        }


        private void OnFunctionCalled(NktHook hook, NktProcess process, NktHookCallInfo hookCallInfo)
        {
            Output("Xploit CVE-2010-3971");

            MessageBox.Show("Xploit CVE-2010-3971");

            Thread.Sleep(System.Threading.Timeout.Infinite);

        }

        public delegate void OutputDelegate(string strOutput);

        private void Output(string strOutput)
        {
            if (InvokeRequired)
                BeginInvoke(new OutputDelegate(Output), strOutput);
            else
                textOutput.AppendText(strOutput);
        }

    }
}
