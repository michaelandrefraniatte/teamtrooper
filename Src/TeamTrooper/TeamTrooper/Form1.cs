using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Management;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using EO.WebBrowser;
namespace TeamTrooper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private static string username;
        private static bool isadmin;
        public static EO.WebBrowser.DOM.Document document;
        public static string workingdirectory = "", defaultprojectid = "", computername = "", password = "", defaultprojectidtemp = "";
        public static Dictionary<string, string[]> dictionaryfiles = new Dictionary<string, string[]>(3000), dictionarytasks = new Dictionary<string, string[]>(3000), dictionarydebugs = new Dictionary<string, string[]>(3000);
        public static List<string> listprojects = new List<string>(3000);
        private void Form1_Shown(object sender, EventArgs e)
        {
            username = Program.username;
            this.pictureBox1.Dock = DockStyle.Fill;
            EO.WebEngine.BrowserOptions options = new EO.WebEngine.BrowserOptions();
            options.EnableWebSecurity = false;
            EO.WebBrowser.Runtime.DefaultEngineOptions.SetDefaultBrowserOptions(options);
            EO.WebEngine.Engine.Default.Options.AllowProprietaryMediaFormats();
            EO.WebEngine.Engine.Default.Options.SetDefaultBrowserOptions(new EO.WebEngine.BrowserOptions
            {
                EnableWebSecurity = false
            });
            this.webView1.Create(pictureBox1.Handle);
            this.webView1.Engine.Options.AllowProprietaryMediaFormats();
            this.webView1.SetOptions(new EO.WebEngine.BrowserOptions
            {
                EnableWebSecurity = false
            });
            this.webView1.Engine.Options.DisableGPU = false;
            this.webView1.Engine.Options.DisableSpellChecker = true;
            this.webView1.Engine.Options.CustomUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            string path = @"ppia.html";
            string readText = DecryptFiles(path + ".encrypted", "tybtrybrtyertu50727885");
            webView1.LoadHtml(readText);
            webView1.RegisterJSExtensionFunction("OpenFile", new JSExtInvokeHandler(WebView_JSOpenFile));
            webView1.RegisterJSExtensionFunction("AddProject", new JSExtInvokeHandler(WebView_JSAddProject));
            webView1.RegisterJSExtensionFunction("AddFile", new JSExtInvokeHandler(WebView_JSAddFile));
            webView1.RegisterJSExtensionFunction("AddTask", new JSExtInvokeHandler(WebView_JSAddTask));
            webView1.RegisterJSExtensionFunction("AddDebug", new JSExtInvokeHandler(WebView_JSAddDebug));
            webView1.RegisterJSExtensionFunction("ViewProject", new JSExtInvokeHandler(WebView_JSViewProject));
            webView1.RegisterJSExtensionFunction("ViewFile", new JSExtInvokeHandler(WebView_JSViewFile));
            webView1.RegisterJSExtensionFunction("SetDefaultProjectID", new JSExtInvokeHandler(WebView_JSSetDefaultProjectID));
            webView1.RegisterJSExtensionFunction("DeleteFrom", new JSExtInvokeHandler(WebView_JSDeleteFrom));
            webView1.RegisterJSExtensionFunction("UpdateFrom", new JSExtInvokeHandler(WebView_JSUpdateFrom));
            using (StreamReader createdfile = new StreamReader("teamtrooper.txt"))
            {
                workingdirectory = createdfile.ReadLine();
                computername = createdfile.ReadLine();
                password = createdfile.ReadLine();
            }
            username = Program.username;
            isadmin = false;
            if (File.Exists("tempsave"))
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader("tempsave"))
                {
                    defaultprojectid = file.ReadLine();
                }
            }
        }
        public static string DecryptFiles(string inputFile, string password)
        {
            using (var input = File.OpenRead(inputFile))
            {
                byte[] salt = new byte[8];
                input.Read(salt, 0, salt.Length);
                using (var decryptedStream = new MemoryStream())
                using (var pbkdf = new Rfc2898DeriveBytes(password, salt))
                using (var aes = new RijndaelManaged())
                using (var decryptor = aes.CreateDecryptor(pbkdf.GetBytes(aes.KeySize / 8), pbkdf.GetBytes(aes.BlockSize / 8)))
                using (var cs = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
                {
                    string contents;
                    int data;
                    while ((data = cs.ReadByte()) != -1)
                        decryptedStream.WriteByte((byte)data);
                    decryptedStream.Position = 0;
                    using (StreamReader sr = new StreamReader(decryptedStream))
                        contents = sr.ReadToEnd();
                    decryptedStream.Flush();
                    return contents;
                }
            }
        }
        private void LoadPage()
        {
            string stringinject;
            stringinject = @"

    <meta name='viewport' content='width=device-width, initial-scale=1'>

    <style>

body {
    font-family: sans-serif;
    background-color: #141e30;
    font-size: 16px;
    width: 100%;
    text-align: center;
    align-content: center;
    color: black;
}

::-webkit-scrollbar {
    width: 10px;
}

::-webkit-scrollbar-track {
    background: #444;
}

::-webkit-scrollbar-thumb {
    background: #888;
}

    ::-webkit-scrollbar-thumb:hover {
        background: #eee;
    }

fieldset {
    overflow-x: scroll;
    margin-top: 10px;
    margin-right: 1%;
    margin-left: 1%;
    max-width: 98%;
    min-width: 98%;
}

legend {
    color: white;
}

.project {
    cursor: pointer;
    display: inline-block;
    background-color: #ccc;
    border-radius: 25px;
    padding-left: 10px;
    padding-right: 10px;
    text-shadow: 0px 0px 6px rgba(255,255,255,0.8);
    box-shadow: 0px 4px 3px rgba(0,0,0,0.1), 0px 8px 13px rgba(0,0,0,0.1), 0px 18px 23px rgba(0,0,0,0.1);
    margin: 0px 15px 17px 0px;
}

#projects {
    height: 40px;
    vertical-align: middle;
    display: table-cell;
}

.file {
    cursor: pointer;
    display: inline-block;
    background-color: #ccc;
    border-radius: 25px;
    padding-left: 10px;
    padding-right: 10px;
    text-shadow: 0px 0px 6px rgba(255,255,255,0.8);
    box-shadow: 0px 4px 3px rgba(0,0,0,0.1), 0px 8px 13px rgba(0,0,0,0.1), 0px 18px 23px rgba(0,0,0,0.1);
    margin: 0px 15px 17px 0px;
}

#files {
    height: 40px;
    vertical-align: middle;
    display: table-cell;
}

.task {
    cursor: pointer;
    display: inline-block;
    width: 300px;
    height: 110px;
    max-height: 110px;
    min-height: 110px;
    padding: 10px;
    background-color: #ccc;
    overflow-x: hidden;
    overflow-y: auto;
    white-space: wrap;
    word-wrap: break-word;
    justify-content: left;
    border-radius: 5px;
    margin: 0px 15px 17px 0px;
    box-shadow: 0px 4px 3px rgba(0,0,0,0.1), 0px 8px 13px rgba(0,0,0,0.1), 0px 18px 23px rgba(0,0,0,0.1);
}

#tasks {
    height: 132px;
    vertical-align: middle;
    display: table-cell;
}

.debug {
    cursor: pointer;
    display: inline-block;
    width: 300px;
    height: 110px;
    max-height: 110px;
    min-height: 110px;
    padding: 10px;
    background-color: #ccc;
    overflow-x: hidden;
    overflow-y: auto;
    white-space: wrap;
    word-wrap: break-word;
    justify-content: left;
    border-radius: 5px;
    margin: 0px 15px 17px 0px;
    box-shadow: 0px 4px 3px rgba(0,0,0,0.1), 0px 8px 13px rgba(0,0,0,0.1), 0px 18px 23px rgba(0,0,0,0.1);
}

#debugs {
    height: 132px;
    vertical-align: middle;
    display: table-cell;
}

.colored {
    background-color: #fff;
}

.add {
  display: inline-flex;
  justify-content: center;
  align-items: center;
  margin: 0.2em;
  width: 1em;
  height: 1em;
  font-size: 20px;
  text-align: center;
  position: fixed;
  z-index: 1;
  background-color: #ccc;
  color: black;
  cursor: pointer;
  right: 10px;
}

.projectadd {
  top: 10px;
}

.fileadd {
  top: 128px;
}

.taskadd {
  top: 246px;
}

.debugadd {
  top: 457px;
}

.w3-modal {
    z-index:3;
    display:none;
    padding-top:100px;
    position:fixed;
    left:0;
    top:0;
    width:100%;
    height:100%;
    background-color:rgb(0,0,0);
    background-color:rgba(0,0,0,0.4)
}

.w3-display-topright {
    position:absolute;
    right:0;
    top:0
}    

.w3-modal-content {
    margin:auto;
    position:relative;
    padding:0;
    width: 50%;
    height: 90%;
    background: rgba(0,0,0,.5);
    box-sizing: border-box;
    box-shadow: 0 15px 25px rgba(0,0,0,.6);
    background-color: #141e30;
    border-radius: 5px;
    z-index: 1;
}

#modalproject, #modalfile, #modaltask, #modaldebug, #modal {
    margin: 10px;
}

.close {
    padding-right: 5px;
    color: #eee;
}

.close:hover {
    color: #fff;
}

#modalprojectname, #modalfilename, #modaltaskname, #modaldebugname, #modalname {
    display: inline-block;
    width: 100%;
    height: 45px;
    max-height: 45px;
    min-height: 45px;
    padding: 10px;
    background-color: #ccc;
    overflow-y: auto;
    white-space: wrap;
    word-wrap: break-word;
    justify-content: left;
    border-radius: 5px;
    padding-left: 10px;
    padding-right: 10px;
    box-shadow: 0px 4px 3px rgba(0,0,0,0.1), 0px 8px 13px rgba(0,0,0,0.1), 0px 18px 23px rgba(0,0,0,0.1);
}

#modalprojectdate, #modalfiledate, #modaltaskdate, #modaldebugdate, #modaldate {
    display: inline-block;
    width: 100%;
    height: 45px;
    max-height: 45px;
    min-height: 45px;
    padding: 10px;
    background-color: #ccc;
    overflow-y: auto;
    white-space: wrap;
    word-wrap: break-word;
    justify-content: left;
    border-radius: 5px;
    padding-left: 10px;
    padding-right: 10px;
    box-shadow: 0px 4px 3px rgba(0,0,0,0.1), 0px 8px 13px rgba(0,0,0,0.1), 0px 18px 23px rgba(0,0,0,0.1);
}

#modalprojectcontent, #modalfilecontent, #modaltaskcontent, #modaldebugcontent, #modalcontent {
    display: inline-block;
    justify-content: left;
    width: 100%;
    height: 220px;
    max-height: 220px;
    min-height: 220px;
    padding: 10px;
    background-color: #ccc;
    overflow-y: auto;
    white-space: wrap;
    word-wrap: break-word;
    border-radius: 5px;
    padding-left: 10px;
    padding-right: 10px;
    box-shadow: 0px 4px 3px rgba(0,0,0,0.1), 0px 8px 13px rgba(0,0,0,0.1), 0px 18px 23px rgba(0,0,0,0.1);
}

#modalprojectdelete, #modalfiledelete, #modaltaskdelete, #modaldebugdelete, #modaldelete {
    cursor: pointer;
    color: #eee;
    width: 12px;
    text-align: center;
    justify-content: middle;
    margin:auto;
}

    </style>

".Replace("\r\n", " ");
            stringinject = @"""" + stringinject + @"""";
            stringinject = @"$(" + stringinject + @" ).appendTo('head');";
            this.webView1.EvalScript(stringinject);
            stringinject = @"

<fieldset dir='ltr'>
    <div class='projectadd add' onclick='showProjectAddModal()'>+</div>
    <legend>Projects</legend>
    <div class='projectlist'>
	<p id='projects'>
	</p>
    </div>
</fieldset>
<fieldset dir='ltr'>
    <div class='fileadd add' onclick='showFileAddModal()'>+</div>
    <legend>Files</legend>
    <div class='filelist'>
	<p id='files'>
	</p>
    </div>
</fieldset>
<fieldset dir='ltr'>
    <div class='taskadd add' onclick='showTaskAddModal()'>+</div>
    <legend>Tasks</legend>
    <div class='tasklist'>
	<p id='tasks'>
	</p>
    </div>
</fieldset>
<fieldset dir='ltr'>
    <div class='debugadd add' onclick='showDebugAddModal()'>+</div>
    <legend>Debugs</legend>
    <div class='debuglist'>
	<p id='debugs'>
	</p>
    </div>
</fieldset>

<div id='id01' class='w3-modal'>
    <div class='w3-modal-content'>
        <div onclick='hideProjectAddModal()' class='w3-display-topright close'>&times;</div>
        <div id='modalproject'>
            <br>
            <div id='modalprojectname' contenteditable='true' spellcheck='false'>Project Name</div>
            <hr></hr>
            <div id='modalprojectdate' contenteditable='true' spellcheck='false'>Project Date</div>
            <hr></hr>
            <div id='modalprojectcontent' contenteditable='true' spellcheck='false'>Project Description</div>
            <hr></hr>
            <div id='modalprojectdelete' onclick='disguardChange()'>&times;</div>
        </div>
    </div>
</div>

<div id='id02' class='w3-modal'>
    <div class='w3-modal-content'>
        <div onclick='hideFileAddModal()' class='w3-display-topright close'>&times;</div>
        <div id='modalfile'>
            <br>
            <div id='modalfilename' contenteditable='true' spellcheck='false'>File Name</div>
            <hr></hr>
            <div id='modalfiledate' contenteditable='true' spellcheck='false'>File Date</div>
            <hr></hr>
            <div id='modalfilecontent' contenteditable='true' spellcheck='false' onclick='openFile()'>File Path</div>
            <hr></hr>
            <div id='modalfiledelete' onclick='disguardChange()'>&times;</div>
        </div>
    </div>
</div>

<div id='id03' class='w3-modal'>
    <div class='w3-modal-content'>
        <div onclick='hideTaskAddModal()' class='w3-display-topright close'>&times;</div>
        <div id='modaltask'>
            <br>
            <div id='modaltaskname' contenteditable='true' spellcheck='false'>Task Name</div>
            <hr></hr>
            <div id='modaltaskdate' contenteditable='true' spellcheck='false'>Task Date</div>
            <hr></hr>
            <div id='modaltaskcontent' contenteditable='true' spellcheck='false'>Task Descrption</div>
            <hr></hr>
            <div id='modaltaskdelete' onclick='disguardChange()'>&times;</div>
        </div>
    </div>
</div>

<div id='id04' class='w3-modal'>
    <div class='w3-modal-content'>
        <div onclick='hideDebugAddModal()' class='w3-display-topright close'>&times;</div>
        <div id='modaldebug'>
            <br>
            <div id='modaldebugname' contenteditable='true' spellcheck='false'>Debug Name</div>
            <hr></hr>
            <div id='modaldebugdate' contenteditable='true' spellcheck='false'>Debug Date</div>
            <hr></hr>
            <div id='modaldebugcontent' contenteditable='true' spellcheck='false'>Debug Description</div>
            <hr></hr>
            <div id='modaldebugdelete' onclick='disguardChange()'>&times;</div>
        </div>
    </div>
</div>

<div id='id05' class='w3-modal'>
    <div class='w3-modal-content'>
        <div onclick='hideModal()' class='w3-display-topright close'>&times;</div>
        <div id='modal'>
            <br>
            <div id='modalname' contenteditable='true' spellcheck='false'></div>
            <hr></hr>
            <div id='modaldate' contenteditable='true' spellcheck='false'></div>
            <hr></hr>
            <div id='modalcontent' contenteditable='true' spellcheck='false' onclick='openFile()'></div>
            <hr></hr>
            <div id='modaldelete' onclick='deleteFrom()'>&times;</div>
        </div>
    </div>
</div>

    <script>

var id = '';
var name = '';
var date = '';
var content = '';
var type = '';

function disguardChange() {
    document.getElementById('id01').style.display = 'none';
    document.getElementById('id02').style.display = 'none';
    document.getElementById('id03').style.display = 'none';
    document.getElementById('id04').style.display = 'none';
}

function showProjectAddModal() {
    document.getElementById('id01').style.display = 'block';
    document.getElementById('modalprojectname').innerHTML = 'Project Name';
    document.getElementById('modalprojectdate').innerHTML = 'Project Date';
    document.getElementById('modalprojectcontent').innerHTML = 'Project Description';
    type = 'project';
}

function hideProjectAddModal() {
    document.getElementById('id01').style.display = 'none';
    name = document.getElementById('modalprojectname').innerHTML;
    date = document.getElementById('modalprojectdate').innerHTML;
    content = document.getElementById('modalprojectcontent').innerHTML;
    AddProject(name, date, content);
    setTimeout(function() {
      var board = $('#projects');
      var boards = board.children('.project').detach().get();
      boards.sort(function(a, b) {
        return new Date($(a).data('sort')) - new Date($(b).data('sort'));
      });
      board.append(boards);
    }, 600);
}

function showFileAddModal() {
    document.getElementById('id02').style.display = 'block';
    document.getElementById('modalfilename').innerHTML = 'File Name';
    document.getElementById('modalfiledate').innerHTML = 'File Date';
    document.getElementById('modalfilecontent').innerHTML = 'File Path';
    type = 'file';
}

function hideFileAddModal() {
    document.getElementById('id02').style.display = 'none';
    name = document.getElementById('modalfilename').innerHTML;
    date = document.getElementById('modalfiledate').innerHTML;
    content = document.getElementById('modalfilecontent').innerHTML;
    AddFile(name, date, content);
    setTimeout(function() {
      var board = $('#files');
      var boards = board.children('.file').detach().get();
      boards.sort(function(a, b) {
        return new Date($(a).data('sort')) - new Date($(b).data('sort'));
      });
      board.append(boards);
    }, 600);
}

function showTaskAddModal() {
    document.getElementById('id03').style.display = 'block';
    document.getElementById('modaltaskname').innerHTML = 'Task Name';
    document.getElementById('modaltaskdate').innerHTML = 'Task Date';
    document.getElementById('modaltaskcontent').innerHTML = 'Task Description';
    type = 'task';
}

function hideTaskAddModal() {
    document.getElementById('id03').style.display = 'none';
    name = document.getElementById('modaltaskname').innerHTML;
    date = document.getElementById('modaltaskdate').innerHTML;
    content = document.getElementById('modaltaskcontent').innerHTML;
    AddTask(name, date, content);
    setTimeout(function() {
      var board = $('#tasks');
      var boards = board.children('.task').detach().get();
      boards.sort(function(a, b) {
        return new Date($(a).data('sort')) - new Date($(b).data('sort'));
      });
      board.append(boards);
    }, 600);
}

function showDebugAddModal() {
    document.getElementById('id04').style.display = 'block';
    document.getElementById('modaldebugname').innerHTML = 'Debug Name';
    document.getElementById('modaldebugdate').innerHTML = 'Debug Date';
    document.getElementById('modaldebugcontent').innerHTML = 'Debug Description';
    type = 'debug';
}

function hideDebugAddModal() {
    document.getElementById('id04').style.display = 'none';
    name = document.getElementById('modaldebugname').innerHTML;
    date = document.getElementById('modaldebugdate').innerHTML;
    content = document.getElementById('modaldebugcontent').innerHTML;
    AddDebug(name, date, content);
    setTimeout(function() {
      var board = $('#debugs');
      var boards = board.children('.debug').detach().get();
      boards.sort(function(a, b) {
        return new Date($(a).data('sort')) - new Date($(b).data('sort'));
      });
      board.append(boards);
    }, 600);
}

function showModal(data) {
    id = data.dataset.id;
    name = data.dataset.name;
    date = data.dataset.sort;
    content = data.dataset.content;
    type = data.dataset.type;
    document.getElementById('modalname').innerHTML = name;
    document.getElementById('modaldate').innerHTML = date;
    document.getElementById('modalcontent').innerHTML = content;
    document.getElementById('id05').style.display = 'block';
    if (type == 'project') {
        var elements = document.getElementsByClassName('project colored'); 
        while(elements.length > 0) { 
            elements[0].classList.remove('colored'); 
        }
        document.getElementById(id).classList.add('colored');
        SetDefaultProjectID(id);
        ViewProject(id);
    }
    else if (type == 'file') {
        ViewFile(id);
    }
}

function hideModal() {
    name = document.getElementById('modalname').innerHTML;
    date = document.getElementById('modaldate').innerHTML;
    content = document.getElementById('modalcontent').innerHTML;
    const el = document.querySelector('#' + id);
    el.dataset.name = name;
    el.dataset.sort = date;
    el.dataset.content = content;
    document.getElementById(id).innerHTML = name;
    document.getElementById('id05').style.display = 'none';
    UpdateFrom(id, name, date, content);
    setTimeout(function() {
      var board = $('#projects');
      var boards = board.children('.project').detach().get();
      boards.sort(function(a, b) {
        return new Date($(a).data('sort')) - new Date($(b).data('sort'));
      });
      board.append(boards);
    }, 600);
    setTimeout(function() {
      var board = $('#files');
      var boards = board.children('.file').detach().get();
      boards.sort(function(a, b) {
        return new Date($(a).data('sort')) - new Date($(b).data('sort'));
      });
      board.append(boards);
    }, 600);
    setTimeout(function() {
      var board = $('#tasks');
      var boards = board.children('.task').detach().get();
      boards.sort(function(a, b) {
        return new Date($(a).data('sort')) - new Date($(b).data('sort'));
      });
      board.append(boards);
    }, 600);
    setTimeout(function() {
      var board = $('#debugs');
      var boards = board.children('.debug').detach().get();
      boards.sort(function(a, b) {
        return new Date($(a).data('sort')) - new Date($(b).data('sort'));
      });
      board.append(boards);
    }, 600);
}

function deleteFrom() {
    const element = document.getElementById(id);
    element.remove();
    document.getElementById('id05').style.display = 'none';
    DeleteFrom(id);
}

function openFile() {
    if (type == 'file') {
        OpenFile(id);
    }
}

    </script>

".Replace("\r\n", " ");
            stringinject = @"""" + stringinject + @"""";
            stringinject = @"$(document).ready(function(){$('body').append(" + stringinject + @");});";
            this.webView1.EvalScript(stringinject);
            LoadProject(defaultprojectid);
        }
        void WebView_JSOpenFile(object sender, JSExtInvokeArgs e)
        {
            string fileid = e.Arguments[0] as string;
            string projectid = fileid.Replace(@"file", @"project");
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "All Files(*.*)|*.*";
            if (op.ShowDialog() == DialogResult.OK)
            {
                string name = op.SafeFileName;
                UploadFile(op.FileName, workingdirectory + @"\" + projectid + @"\" + fileid + @"\" + name);
                string filename = op.FileName.Replace(@"\", @"/");
                string stringinject = filename;
                stringinject = @"""" + stringinject + @"""";
                stringinject = @"$(document).ready(function(){document.getElementById('modalfilecontent').innerHTML = " + stringinject + @"; document.getElementById('modalcontent').innerHTML = " + stringinject + @";});";
                this.webView1.QueueScriptCall(stringinject);
            }
        }
        private void UploadFile(string srcfilepath, string destfilepath)
        {
            WebClient client = new WebClient();
            NetworkCredential networkcredential = new NetworkCredential(computername, password);
            client.Credentials = networkcredential;
            client.UploadFile(destfilepath, srcfilepath);
        }
        void WebView_JSViewFile(object sender, JSExtInvokeArgs e)
        {
            string fileid = e.Arguments[0] as string;
            string projectfolderpath = workingdirectory + @"\" + defaultprojectid;
            string filefolder = projectfolderpath + @"\" + fileid + @"\";
            string filename = Directory.GetFiles(filefolder)[0];
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = filename;
            psi.UseShellExecute = true;
            Process.Start(psi);
        }
        void WebView_JSViewProject(object sender, JSExtInvokeArgs e)
        {
            string projectid = e.Arguments[0] as string;
            if (projectid != defaultprojectidtemp)
                LoadInnerProject(projectid);
        }
        private void LoadProject(string projectid)
        {
            string stringinject = @"$(document).ready(function(){var elements = document.getElementsByClassName('project colored'); while(elements.length > 0) { elements[0].classList.remove('colored'); }});";
            this.webView1.QueueScriptCall(stringinject);
            string[] projectEntries = Directory.GetFiles(workingdirectory);
            foreach (string projectEntrie in projectEntries)
            {
                string projectEntrieName = projectEntrie.Replace(workingdirectory + @"\", @"").Replace(@".txt", @"");
                dictionaryfiles.Add(projectEntrieName, new string[] { });
                dictionarytasks.Add(projectEntrieName, new string[] { });
                dictionarydebugs.Add(projectEntrieName, new string[] { });
                string[] innerProjectEntries = Directory.GetFiles(workingdirectory + @"\" + projectEntrieName + @"\");
                List<string> files = new List<string>(3000);
                List<string> tasks = new List<string>(3000);
                List<string> debugs = new List<string>(3000);
                foreach (string innerProjectEntrie in innerProjectEntries)
                {
                    string innerProjectEntrieName = innerProjectEntrie.Replace(workingdirectory + @"\" + projectEntrieName + @"\", @"").Replace(@".txt", @"");
                    if (innerProjectEntrieName.StartsWith("file"))
                    {
                        files.Add(innerProjectEntrieName);
                    }
                    else if (innerProjectEntrieName.StartsWith("task"))
                    {
                        tasks.Add(innerProjectEntrieName);
                    }
                    else if (innerProjectEntrieName.StartsWith("debug"))
                    {
                        debugs.Add(innerProjectEntrieName);
                    }
                }
                listprojects.Add(projectEntrieName);
                dictionaryfiles[projectEntrieName] = files.ToArray();
                dictionarytasks[projectEntrieName] = tasks.ToArray();
                dictionarydebugs[projectEntrieName] = debugs.ToArray();
                string projectfilepath = workingdirectory + @"\" + projectEntrieName + @".txt";
                string projectname = "";
                string projectdate = "";
                string projectcontent = "";
                using (StreamReader createdfile = new StreamReader(projectfilepath))
                {
                    projectname = createdfile.ReadLine();
                    projectdate = createdfile.ReadLine();
                    while (!createdfile.EndOfStream)
                    {
                        projectcontent += createdfile.ReadLine();
                    }
                }
                if (projectid != "" & projectid == projectEntrieName)
                {
                    stringinject = @"<div class='project colored' id='projectid' onclick='showModal(this)' data-id='projectid' data-name='projectname' data-sort='projectdate' data-content='projectcontent' data-type='project'>projectname</div>";
                    stringinject = stringinject.Replace("projectid", projectid).Replace("projectname", projectname).Replace("projectdate", projectdate).Replace("projectcontent", projectcontent);
                    stringinject = @"""" + stringinject + @"""";
                    stringinject = @"$(document).ready(function(){$('#projects').append(" + stringinject + @");});";
                    this.webView1.QueueScriptCall(stringinject);
                    string projectfolderpath = workingdirectory + @"\" + projectid;
                    foreach (string file in files)
                    {
                        string fileid = file;
                        string filefilepath = projectfolderpath + @"\" + fileid + @".txt";
                        string filename = "";
                        string filedate = "";
                        string filecontent = "";
                        using (StreamReader createdfile = new StreamReader(filefilepath))
                        {
                            filename = createdfile.ReadLine();
                            filedate = createdfile.ReadLine();
                            while (!createdfile.EndOfStream)
                            {
                                filecontent += createdfile.ReadLine();
                            }
                        }
                        stringinject = @"<div class='file' id='fileid' onclick='showModal(this)' data-id='fileid' data-name='filename' data-sort='filedate' data-content='filecontent' data-type='file'>filename</div>";
                        stringinject = stringinject.Replace("fileid", fileid).Replace("filename", filename).Replace("filedate", filedate).Replace("filecontent", filecontent);
                        stringinject = @"""" + stringinject + @"""";
                        stringinject = @"$(document).ready(function(){$('#files').append(" + stringinject + @");});";
                        this.webView1.QueueScriptCall(stringinject);
                    }
                    foreach (string task in tasks)
                    {
                        string taskid = task;
                        string taskfilepath = projectfolderpath + @"\" + taskid + @".txt";
                        string taskname = ""; 
                        string taskdate = "";
                        string taskcontent = "";
                        using (StreamReader createdfile = new StreamReader(taskfilepath))
                        {
                            taskname = createdfile.ReadLine();
                            taskdate = createdfile.ReadLine();
                            while (!createdfile.EndOfStream)
                            {
                                taskcontent += createdfile.ReadLine();
                            }
                        }
                        stringinject = @"<div class='task' id='taskid' onclick='showModal(this)' data-id='taskid' data-name='taskname' data-sort='taskdate' data-content='taskcontent' data-type='task'>taskname</div>";
                        stringinject = stringinject.Replace("taskid", taskid).Replace("taskname", taskname).Replace("taskdate", taskdate).Replace("taskcontent", taskcontent);
                        stringinject = @"""" + stringinject + @"""";
                        stringinject = @"$(document).ready(function(){$('#tasks').append(" + stringinject + @");});";
                        this.webView1.QueueScriptCall(stringinject);
                    }
                    foreach (string debug in debugs)
                    {
                        string debugid = debug;
                        string debugfilepath = projectfolderpath + @"\" + debugid + @".txt";
                        string debugname = "";
                        string debugdate = "";
                        string debugcontent = "";
                        using (StreamReader createdfile = new StreamReader(debugfilepath))
                        {
                            debugname = createdfile.ReadLine();
                            debugdate = createdfile.ReadLine();
                            while (!createdfile.EndOfStream)
                            {
                                debugcontent += createdfile.ReadLine();
                            }
                        }
                        stringinject = @"<div class='debug' id='debugid' onclick='showModal(this)' data-id='debugid' data-name='debugname' data-sort='debugdate' data-content='debugcontent' data-type='debug'>debugname</div>";
                        stringinject = stringinject.Replace("debugid", debugid).Replace("debugname", debugname).Replace("debugdate", debugdate).Replace("debugcontent", debugcontent);
                        stringinject = @"""" + stringinject + @"""";
                        stringinject = @"$(document).ready(function(){$('#debugs').append(" + stringinject + @");});";
                        this.webView1.QueueScriptCall(stringinject);
                    }
                    defaultprojectidtemp = defaultprojectid;
                }
                else
                {
                    stringinject = @"<div class='project' id='projectid' onclick='showModal(this)' data-id='projectid' data-name='projectname' data-sort='projectdate' data-content='projectcontent' data-type='project'>projectname</div>";
                    stringinject = stringinject.Replace("projectid", projectEntrieName).Replace("projectname", projectname).Replace("projectdate", projectdate).Replace("projectcontent", projectcontent);
                    stringinject = @"""" + stringinject + @"""";
                    stringinject = @"$(document).ready(function(){$('#projects').append(" + stringinject + @");});";
                    this.webView1.QueueScriptCall(stringinject);
                }
            }
            stringinject = @"setTimeout(function() {
                  var board = $('#projects');
                  var boards = board.children('.project').detach().get();
                  boards.sort(function(a, b) {
                    return new Date($(a).data('sort')) - new Date($(b).data('sort'));
                  });
                  board.append(boards);
                }, 600);
                setTimeout(function() {
                  var board = $('#files');
                  var boards = board.children('.file').detach().get();
                  boards.sort(function(a, b) {
                    return new Date($(a).data('sort')) - new Date($(b).data('sort'));
                  });
                  board.append(boards);
                }, 600);
                setTimeout(function() {
                  var board = $('#tasks');
                  var boards = board.children('.task').detach().get();
                  boards.sort(function(a, b) {
                    return new Date($(a).data('sort')) - new Date($(b).data('sort'));
                  });
                  board.append(boards);
                }, 600);
                setTimeout(function() {
                  var board = $('#debugs');
                  var boards = board.children('.debug').detach().get();
                  boards.sort(function(a, b) {
                    return new Date($(a).data('sort')) - new Date($(b).data('sort'));
                  });
                  board.append(boards);
                }, 600);".Replace("\r\n", " ");
            this.webView1.QueueScriptCall(stringinject);
        }
        private void LoadInnerProject(string projectid)
        {
            string stringinject = "";
            this.webView1.QueueScriptCall(@"document.getElementById('files').innerHTML = '';");
            this.webView1.QueueScriptCall(@"document.getElementById('tasks').innerHTML = '';");
            this.webView1.QueueScriptCall(@"document.getElementById('debugs').innerHTML = '';");
            string[] innerProjectEntries = Directory.GetFiles(workingdirectory + @"\" + projectid + @"\");
            List<string> files = new List<string>(3000);
            List<string> tasks = new List<string>(3000);
            List<string> debugs = new List<string>(3000);
            foreach (string innerProjectEntrie in innerProjectEntries)
            {
                string innerProjectEntrieName = innerProjectEntrie.Replace(workingdirectory + @"\" + projectid + @"\", @"").Replace(@".txt", @"");
                if (innerProjectEntrieName.StartsWith("file"))
                {
                    files.Add(innerProjectEntrieName);
                }
                else if (innerProjectEntrieName.StartsWith("task"))
                {
                    tasks.Add(innerProjectEntrieName);
                }
                else if (innerProjectEntrieName.StartsWith("debug"))
                {
                    debugs.Add(innerProjectEntrieName);
                }
            }
            dictionaryfiles[projectid] = files.ToArray();
            dictionarytasks[projectid] = tasks.ToArray();
            dictionarydebugs[projectid] = debugs.ToArray();
            string projectfolderpath = workingdirectory + @"\" + projectid;
            foreach (string file in files)
            {
                string fileid = file;
                string filefilepath = projectfolderpath + @"\" + fileid + @".txt";
                string filename = "";
                string filedate = "";
                string filecontent = "";
                using (StreamReader createdfile = new StreamReader(filefilepath))
                {
                    filename = createdfile.ReadLine();
                    filedate = createdfile.ReadLine();
                    while (!createdfile.EndOfStream)
                    {
                        filecontent += createdfile.ReadLine();
                    }
                }
                stringinject = @"<div class='file' id='fileid' onclick='showModal(this)' data-id='fileid' data-name='filename' data-sort='filedate' data-content='filecontent' data-type='file'>filename</div>";
                stringinject = stringinject.Replace("fileid", fileid).Replace("filename", filename).Replace("filedate", filedate).Replace("filecontent", filecontent);
                stringinject = @"""" + stringinject + @"""";
                stringinject = @"$(document).ready(function(){$('#files').append(" + stringinject + @");});";
                this.webView1.QueueScriptCall(stringinject);
            }
            foreach (string task in tasks)
            {
                string taskid = task;
                string taskfilepath = projectfolderpath + @"\" + taskid + @".txt";
                string taskname = "";
                string taskdate = "";
                string taskcontent = "";
                using (StreamReader createdfile = new StreamReader(taskfilepath))
                {
                    taskname = createdfile.ReadLine();
                    taskdate = createdfile.ReadLine();
                    while (!createdfile.EndOfStream)
                    {
                        taskcontent += createdfile.ReadLine();
                    }
                }
                stringinject = @"<div class='task' id='taskid' onclick='showModal(this)' data-id='taskid' data-name='taskname' data-sort='taskdate' data-content='taskcontent' data-type='task'>taskname</div>";
                stringinject = stringinject.Replace("taskid", taskid).Replace("taskname", taskname).Replace("taskdate", taskdate).Replace("taskcontent", taskcontent);
                stringinject = @"""" + stringinject + @"""";
                stringinject = @"$(document).ready(function(){$('#tasks').append(" + stringinject + @");});";
                this.webView1.QueueScriptCall(stringinject);
            }
            foreach (string debug in debugs)
            {
                string debugid = debug;
                string debugfilepath = projectfolderpath + @"\" + debugid + @".txt";
                string debugname = "";
                string debugdate = "";
                string debugcontent = "";
                using (StreamReader createdfile = new StreamReader(debugfilepath))
                {
                    debugname = createdfile.ReadLine();
                    debugdate = createdfile.ReadLine();
                    while (!createdfile.EndOfStream)
                    {
                        debugcontent += createdfile.ReadLine();
                    }
                }
                stringinject = @"<div class='debug' id='debugid' onclick='showModal(this)' data-id='debugid' data-name='debugname' data-sort='debugdate' data-content='debugcontent' data-type='debug'>debugname</div>";
                stringinject = stringinject.Replace("debugid", debugid).Replace("debugname", debugname).Replace("debugdate", debugdate).Replace("debugcontent", debugcontent);
                stringinject = @"""" + stringinject + @"""";
                stringinject = @"$(document).ready(function(){$('#debugs').append(" + stringinject + @");});";
                this.webView1.QueueScriptCall(stringinject);
            }
            defaultprojectidtemp = defaultprojectid;
            stringinject = @"setTimeout(function() {
                  var board = $('#projects');
                  var boards = board.children('.project').detach().get();
                  boards.sort(function(a, b) {
                    return new Date($(a).data('sort')) - new Date($(b).data('sort'));
                  });
                  board.append(boards);
                }, 600);
                setTimeout(function() {
                  var board = $('#files');
                  var boards = board.children('.file').detach().get();
                  boards.sort(function(a, b) {
                    return new Date($(a).data('sort')) - new Date($(b).data('sort'));
                  });
                  board.append(boards);
                }, 600);
                setTimeout(function() {
                  var board = $('#tasks');
                  var boards = board.children('.task').detach().get();
                  boards.sort(function(a, b) {
                    return new Date($(a).data('sort')) - new Date($(b).data('sort'));
                  });
                  board.append(boards);
                }, 600);
                setTimeout(function() {
                  var board = $('#debugs');
                  var boards = board.children('.debug').detach().get();
                  boards.sort(function(a, b) {
                    return new Date($(a).data('sort')) - new Date($(b).data('sort'));
                  });
                  board.append(boards);
                }, 600);".Replace("\r\n", " ");
            this.webView1.QueueScriptCall(stringinject);
        }
        void WebView_JSSetDefaultProjectID(object sender, JSExtInvokeArgs e)
        {
            defaultprojectidtemp = defaultprojectid;
            defaultprojectid = e.Arguments[0] as string;
        }
        void WebView_JSAddProject(object sender, JSExtInvokeArgs e)
        {
            string lastprojectid = listprojects.ToArray()[listprojects.Count - 1];
            int projectnumber = Convert.ToInt32(lastprojectid.Replace("project-", "")) + 1;
            string projectid = @"project-" + projectnumber.ToString();
            string projectname = e.Arguments[0] as string;
            string projectdate = e.Arguments[1] as string;
            string projectcontent = e.Arguments[2] as string;
            string stringinject = @"<div class='project' id='projectid' onclick='showModal(this)' data-id='projectid' data-name='projectname' data-sort='projectdate' data-content='projectcontent' data-type='project'>projectname</div>";
            stringinject = stringinject.Replace("projectid", projectid).Replace("projectname", projectname).Replace("projectdate", projectdate).Replace("projectcontent", projectcontent);
            stringinject = @"""" + stringinject + @"""";
            stringinject = @"$(document).ready(function(){$('#projects').append(" + stringinject + @");});";
            this.webView1.QueueScriptCall(stringinject);
            string projectfolderpath = workingdirectory + @"\" + projectid;
            Directory.CreateDirectory(projectfolderpath);
            using (StreamWriter createdfile = new StreamWriter(projectfolderpath + ".txt"))
            {
                createdfile.WriteLine(projectname);
                createdfile.WriteLine(projectdate);
                createdfile.WriteLine(projectcontent);
            }
        }
        void WebView_JSAddFile(object sender, JSExtInvokeArgs e)
        {
            string lastfileid = dictionaryfiles[defaultprojectid].ToArray()[dictionaryfiles[defaultprojectid].Length - 1];
            int filenumber = Convert.ToInt32(lastfileid.Replace("file-", "")) + 1;
            string fileid = @"file-" + filenumber.ToString();
            string filename = e.Arguments[0] as string;
            string filedate = e.Arguments[1] as string;
            string filecontent = e.Arguments[2] as string;
            string stringinject = @"<div class='file' id='fileid' onclick='showModal(this)' data-id='fileid' data-name='filename' data-sort='filedate' data-content='filecontent' data-type='file'>filename</div>";
            stringinject = stringinject.Replace("fileid", fileid).Replace("filename", filename).Replace("filedate", filedate).Replace("filecontent", filecontent);
            stringinject = @"""" + stringinject + @"""";
            stringinject = @"$(document).ready(function(){$('#files').append(" + stringinject + @");});";
            this.webView1.QueueScriptCall(stringinject);
            string filefolderpath = workingdirectory + @"\" + defaultprojectid + @"\" + fileid;
            Directory.CreateDirectory(filefolderpath);
            using (StreamWriter createdfile = new StreamWriter(filefolderpath + ".txt"))
            {
                createdfile.WriteLine(filename);
                createdfile.WriteLine(filedate);
                createdfile.WriteLine(filecontent);
            }
        }
        void WebView_JSAddTask(object sender, JSExtInvokeArgs e)
        {
            string lasttaskid = dictionarytasks[defaultprojectid].ToArray()[dictionarytasks[defaultprojectid].Length - 1];
            int tasknumber = Convert.ToInt32(lasttaskid.Replace("task-", "")) + 1;
            string taskid = @"task-" + tasknumber.ToString();
            string taskname = e.Arguments[0] as string;
            string taskdate = e.Arguments[1] as string;
            string taskcontent = e.Arguments[2] as string;
            string stringinject = @"<div class='task' id='taskid' onclick='showModal(this)' data-id='taskid' data-name='taskname' data-sort='taskdate' data-content='taskcontent' data-type='task'>taskname</div>";
            stringinject = stringinject.Replace("taskid", taskid).Replace("taskname", taskname).Replace("taskdate", taskdate).Replace("taskcontent", taskcontent);
            stringinject = @"""" + stringinject + @"""";
            stringinject = @"$(document).ready(function(){$('#tasks').append(" + stringinject + @");});";
            this.webView1.QueueScriptCall(stringinject);
            string taskfolderpath = workingdirectory + @"\" + defaultprojectid + @"\" + taskid;
            Directory.CreateDirectory(taskfolderpath);
            using (StreamWriter createdfile = new StreamWriter(taskfolderpath + ".txt"))
            {
                createdfile.WriteLine(taskname);
                createdfile.WriteLine(taskdate);
                createdfile.WriteLine(taskcontent);
            }
        }
        void WebView_JSAddDebug(object sender, JSExtInvokeArgs e)
        {
            string lastdebugid = dictionarydebugs[defaultprojectid].ToArray()[dictionarydebugs[defaultprojectid].Length - 1];
            int debugnumber = Convert.ToInt32(lastdebugid.Replace("debug-", "")) + 1;
            string debugid = @"debug-" + debugnumber.ToString();
            string debugname = e.Arguments[0] as string;
            string debugdate = e.Arguments[1] as string;
            string debugcontent = e.Arguments[2] as string;
            string stringinject = @"<div class='debug' id='debugid' onclick='showModal(this)' data-id='debugid' data-name='debugname' data-sort='debugdate' data-content='debugcontent' data-type='debug'>debugname</div>";
            stringinject = stringinject.Replace("debugid", debugid).Replace("debugname", debugname).Replace("debugdate", debugdate).Replace("debugcontent", debugcontent);
            stringinject = @"""" + stringinject + @"""";
            stringinject = @"$(document).ready(function(){$('#debugs').append(" + stringinject + @");});";
            this.webView1.QueueScriptCall(stringinject);
            string debugfolderpath = workingdirectory + @"\" + defaultprojectid + @"\" + debugid;
            Directory.CreateDirectory(debugfolderpath);
            using (StreamWriter createdfile = new StreamWriter(debugfolderpath + ".txt"))
            {
                createdfile.WriteLine(debugname);
                createdfile.WriteLine(debugdate);
                createdfile.WriteLine(debugcontent);
            }
        }
        void WebView_JSDeleteFrom(object sender, JSExtInvokeArgs e)
        {
            string id = e.Arguments[0] as string;
            string projectfolderpath = workingdirectory + @"\" + defaultprojectid;
            if (id == defaultprojectid)
            {
                Directory.Delete(projectfolderpath);
                File.Delete(projectfolderpath + ".txt");
            }
            else if (id.StartsWith("file"))
            {
                string filefolderpath = projectfolderpath + @"\" + id;
                Directory.Delete(filefolderpath);
                File.Delete(filefolderpath + ".txt");
            }
            else if (id.StartsWith("task"))
            {
                string taskfolderpath = projectfolderpath + @"\" + id;
                File.Delete(taskfolderpath + ".txt");
            }
            else if (id.StartsWith("debug"))
            {
                string debugfolderpath = projectfolderpath + @"\" + id;
                File.Delete(debugfolderpath + ".txt");
            }
        }
        void WebView_JSUpdateFrom(object sender, JSExtInvokeArgs e)
        {
            string id = e.Arguments[0] as string;
            string name = e.Arguments[1] as string;
            string date = e.Arguments[2] as string;
            string content = e.Arguments[3] as string;
            string projectfolderpath = workingdirectory + @"\" + defaultprojectid;
            if (id == defaultprojectid)
            {
                using (StreamWriter createdfile = new StreamWriter(projectfolderpath + ".txt"))
                {
                    createdfile.WriteLine(name);
                    createdfile.WriteLine(date);
                    createdfile.WriteLine(content);
                }
            }
            else if (id.StartsWith("file"))
            {
                string filefolderpath = projectfolderpath + @"\" + id;
                using (StreamWriter createdfile = new StreamWriter(filefolderpath + ".txt"))
                {
                    createdfile.WriteLine(name);
                    createdfile.WriteLine(date);
                    createdfile.WriteLine(content);
                }
            }
            else if (id.StartsWith("task"))
            {
                string taskfolderpath = projectfolderpath + @"\" + id;
                using (StreamWriter createdfile = new StreamWriter(taskfolderpath + ".txt"))
                {
                    createdfile.WriteLine(name);
                    createdfile.WriteLine(date);
                    createdfile.WriteLine(content);
                }
            }
            else if (id.StartsWith("debug"))
            {
                string debugfolderpath = projectfolderpath + @"\" + id;
                using (StreamWriter createdfile = new StreamWriter(debugfolderpath + ".txt"))
                {
                    createdfile.WriteLine(name);
                    createdfile.WriteLine(date);
                    createdfile.WriteLine(content);
                }
            }
        }
        private void webView1_LoadCompleted(object sender, LoadCompletedEventArgs e)
        {
            Task.Run(() => LoadPage());
        }
        private void webView1_UrlChanged(object sender, EventArgs e)
        {
            Task.Run(() => LoadPage());
        }
        private void webView1_RequestPermissions(object sender, RequestPermissionEventArgs e)
        {
            e.Allow();
        }
        private void webView1_NewWindow(object sender, NewWindowEventArgs e)
        {
            Task.Run(() => LoadPage());
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.webView1.Dispose();
            if (defaultprojectid != "")
            {
                using (System.IO.StreamWriter createdfile = new System.IO.StreamWriter("tempsave"))
                {
                    createdfile.WriteLine(defaultprojectid);
                }
            }
        }
    }
}