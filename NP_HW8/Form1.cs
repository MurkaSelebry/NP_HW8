using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace NP_HW8
{
    public partial class Form1 : Form
    {
        private
        const string ServerUrl = "ftp://ftp.dlptest.com";
        private
        const string Username = "dlpuser";
        private
        const string Password = "rNrKYTX9g7z3RgJRmxWuGHbeu";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadFilesFromServer();
        }

        private void LoadFilesFromServer()
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ServerUrl);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                request.Credentials = new NetworkCredential(Username, Password);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string fileList = reader.ReadToEnd();
                        string[] files = fileList.Split(new string[] {
              "\r\n",
              "\n"
            }, StringSplitOptions.RemoveEmptyEntries);

                        filesListBox.Items.Clear();
                        foreach (string file in files)
                        {
                            filesListBox.Items.Add(file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Load Files Error: {ex.Message}");
            }
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string localFolderPath = folderBrowserDialog.SelectedPath;
                    string selectedFile = filesListBox.SelectedItem?.ToString();
                    if (string.IsNullOrEmpty(selectedFile))
                    {
                        MessageBox.Show("Choose file");
                        return;
                    }

                    string serverFileUrl = ServerUrl + "/" + selectedFile;
                    string localFilePath = Path.Combine(localFolderPath, selectedFile);

                    DownloadFile(serverFileUrl, localFilePath);
                }
            }
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "All Files|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string localFilePath = openFileDialog.FileName;
                    string fileName = Path.GetFileName(localFilePath);

                    string serverFileUrl = ServerUrl + "/" + fileName;

                    UploadFile(serverFileUrl, localFilePath);
                }
            }
        }

        private void DownloadFile(string serverFileUrl, string localFilePath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverFileUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(Username, Password);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (FileStream fileStream = new FileStream(localFilePath, FileMode.Create))
                        {
                            byte[] buffer = new byte[1024];
                            int bytesRead;
                            long totalBytesRead = 0;
                            long fileSize = response.ContentLength;

                            Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Start();

                            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fileStream.Write(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;

                                int progressPercentage = (int)((totalBytesRead / (double)fileSize) * 100);
                                downloadProgressBar.Value = progressPercentage;
                            }

                            stopwatch.Stop();
                            double downloadSpeed = fileSize / stopwatch.Elapsed.TotalSeconds;

                            MessageBox.Show($"Download done!\nFile Size: {fileSize} байтов\nSpeed: {downloadSpeed} байтов/сек");
                            downloadProgressBar.Value = 0;
                        }
                    }
                }
               LoadFilesFromServer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Download Error: {ex.Message}");
            }
        }

        private void UploadFile(string serverFileUrl, string localFilePath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverFileUrl);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(Username, Password);

                using (FileStream fileStream = new FileStream(localFilePath, FileMode.Open))
                {
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long fileSize = fileStream.Length;

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            requestStream.Write(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;

                            int progressPercentage = (int)((totalBytesRead / (double)fileSize) * 100);
                            uploadProgressBar.Value = progressPercentage;
                        }

                        stopwatch.Stop();
                        double uploadSpeed = fileSize / stopwatch.Elapsed.TotalSeconds;

                        MessageBox.Show($"Upload done!\nFile Size: {fileSize} байтов\nSpeed: {uploadSpeed} байтов/сек");
                        uploadProgressBar.Value = 0;
                    }
                }

                LoadFilesFromServer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Upload Error: {ex.Message}");
            }
        }
    }
}