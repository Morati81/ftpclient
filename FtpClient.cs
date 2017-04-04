using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
namespace FTPClientLibrary
{
    public class FtpClient
    {

        public string Destinazione;
        public string UserFTP;
        public string PwdFTP;
        public string FilePath;
        public string FileName;

        private string _destinazione;

       
        public void UploadFile()
        {
            try
            {
                _destinazione = Destinazione; 

                if (!(Destinazione.StartsWith("ftp://") || Destinazione.StartsWith("ftp:\\\\")))
                {
                    _destinazione = "ftp://" + Destinazione; 

                }


                System.Net.FtpWebRequest ftp = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(new Uri(_destinazione));

                ftp = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(new Uri(_destinazione + "/" + FileName));
                ftp.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
                ftp.UseBinary = true;

                ftp.Credentials = new System.Net.NetworkCredential(UserFTP, PwdFTP);
                FileInfo fileInf = new FileInfo(FilePath);

                ftp.ContentLength = fileInf.Length;


                int buffLength = 20480;
                byte[] buff = new byte[buffLength ];
                int contentLen;
                FileStream stream = fileInf.OpenRead();

                Stream requestStream = ftp.GetRequestStream();
                contentLen = stream.Read(buff, 0, buffLength);

              

                while (contentLen != 0)
                {
                    requestStream.Write(buff, 0, contentLen);
                    contentLen = stream.Read(buff, 0, buffLength);
              
                }
                requestStream.Close();
                stream.Close();
              
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public bool CreateFTPDirectory(string directory)
        {

            try
            {
                _destinazione = Destinazione; 

                if (!(Destinazione.StartsWith("ftp://") || Destinazione.StartsWith("ftp:\\\\")))
                {
                    _destinazione = "ftp://" + Destinazione; 

                }

                //create the directory
                FtpWebRequest requestDir = (FtpWebRequest)FtpWebRequest.Create(new Uri(_destinazione + "/" + directory));
                requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
                requestDir.Credentials = new NetworkCredential(UserFTP, PwdFTP);
                requestDir.UsePassive = true;
                requestDir.UseBinary = true;
                requestDir.KeepAlive = false;
                FtpWebResponse response = (FtpWebResponse)requestDir.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();

                return true;
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    response.Close();
                    return true;
                }
                else
                {
                    response.Close();
                    return false;
                }
            }
        }

        public void DownloadFile()
        {
            try
            {
                _destinazione = Destinazione; 

                if (!(Destinazione.StartsWith("ftp://") || Destinazione.StartsWith("ftp:\\\\")))
                {
                    _destinazione = "ftp://" + Destinazione; 

                }

                System.Net.FtpWebRequest ftp = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(new Uri(_destinazione + "/" + FileName));
                ftp.Method = System.Net.WebRequestMethods.Ftp.DownloadFile;
                ftp.UseBinary = true;
                ftp.Credentials = new System.Net.NetworkCredential(UserFTP, PwdFTP);


                ftp = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(new Uri(_destinazione + "/" + FileName));
                ftp.Method = System.Net.WebRequestMethods.Ftp.DownloadFile;
                ftp.UseBinary = true;
                ftp.Credentials = new System.Net.NetworkCredential(UserFTP, PwdFTP);

                ftp.Method = System.Net.WebRequestMethods.Ftp.DownloadFile;
                System.Net.FtpWebResponse response = (System.Net.FtpWebResponse)ftp.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);



                using (StreamWriter writer = new StreamWriter(FilePath))
                {

                    writer.Write(reader.ReadToEnd());
                     
                }

                reader.Close();
                response.Close();


            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public long GetDirectorySizeFTP(string fullFilePath, bool recursive)
        {
            long startDirectorySize = 0;
            List<string> subDirs = new List<string>(), files = new List<string>();

            var directoryFileFound = true;
            try
            {

                //Trying to Get Files and Direcotries from the current FTP Path.
                var request = (FtpWebRequest)WebRequest.Create(fullFilePath);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;
                request.Proxy = null;
                request.Credentials = new NetworkCredential(UserFTP, PwdFTP);

                //List Directory: will get Files and Direcotries in current FTP Path.
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                using (var response = (FtpWebResponse)request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string line = streamReader.ReadLine();
                    while (!string.IsNullOrEmpty(line))
                    {   //Read Stream and add lines as Files names.
                        files.Add(line);
                        line = streamReader.ReadLine();
                    }
                }
            }
            catch { directoryFileFound = false; /* If it fails: it means it's empty or not found. */}

            if (!directoryFileFound) //If file not found just return 0;
                return startDirectorySize;

            //Loop on files strings.
            files.ForEach(file =>
            {
                string[] spfile = file.Split(char.Parse("/"));

                //Set full file's path.
                var filePth = Path.Combine(fullFilePath + "/", spfile[1]);
                try
                {

                    var request = (FtpWebRequest)WebRequest.Create(filePth + "/");

                    request.Method = WebRequestMethods.Ftp.ListDirectory;
                    request.Credentials = new NetworkCredential(UserFTP, PwdFTP);

                    try
                    {
                        using (request.GetResponse())
                        {
                            subDirs.Add(file);
                            return;
                        }
                    }
                    catch (WebException)
                    {
                      
                    }

                    
                    //Try to get the current File size and add it to the current Size.
                    request = (FtpWebRequest)WebRequest.Create(filePth);
                    request.UsePassive = true;
                    request.UseBinary = true;
                    request.KeepAlive = false;
                    request.Proxy = null;
                    request.Credentials = new NetworkCredential(UserFTP, PwdFTP);
                    //I have to use  ListDirectoryDetails because  GetFileSize doesn't work on my ftp
                    request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                    using (var response = (FtpWebResponse)request.GetResponse())
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string s;
                        s = reader.ReadLine();
                        //while ((s = reader.ReadLine()) != null)
                        //{

                            if (s.Substring(0, 1) == "d")
                            {
                                //subDirs.Add(file);
                            }
                            else
                            {
                                var tokens = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                if (tokens.Length > 3)
                                {
                                    startDirectorySize += int.Parse(tokens[4]);
                                }
                            }
                        //}
                        //startDirectorySize += response.ContentLength;
                    }


                   



                }

                catch { throw; } //subDirs.Add(file); da scommentare se si riabilita il GetFileSize
            });

            if (recursive) //If it recursive, It'll loop on Sub Direcotries which found in Main Direcotry.
                subDirs.ForEach(subDir =>
                {
                    string[] spSubDor = subDir.Split(char.Parse("/"));

                    //Set full Sub Directory path.
                    var fullSubDirPath = Path.Combine(fullFilePath + "/", spSubDor[1]);
                    //Get size of the current Sub Directory files then add it's size to Main Size.
                    startDirectorySize += GetDirectorySizeFTP
                        (fullSubDirPath, recursive);
                });

            //Return size of the current Directory Files.
            return startDirectorySize;
        }

        public class FileObject
        {
            public bool IsDirectory { get; set; }
            public string FileName { get; set; }
        }
        public void DeleteFilesAndFolders(string path,bool delDirectory=false)
        {
            try
            {
                if (!DirectoryExists(path)) 
                    return;
                
                if (path != null && (path.StartsWith(@"\\") || path.StartsWith("//")))
                    path = path.Remove(0, 1);
                List<FileObject> files = DirectoryListing(path);
                if (files != null)
                {
                    foreach (FileObject file in files.Where(file => !file.IsDirectory))
                    {
                        DeleteFile(path, file.FileName);
                    }

                    foreach (FileObject file in files.Where(file => file.IsDirectory))
                    {
                        DeleteFilesAndFolders(path + "/" + file.FileName);
                        DeleteFolder(path + "/" + file.FileName);
                    }
                }
                if (delDirectory)
                    DeleteFolder(path);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public void DeleteFile(string path, string file)
        {
            try
            {


                _destinazione = Destinazione; 

                if (!(Destinazione.StartsWith("ftp://") || Destinazione.StartsWith("ftp:\\\\")))
                {
                    _destinazione = "ftp://" + Destinazione; 

                }
                string dest = _destinazione;
                if (!path.Equals("")) dest += "/" + path;
                dest += "/" + file;

                var clsRequest = (FtpWebRequest)WebRequest.Create(dest );
                clsRequest.Credentials = new NetworkCredential(UserFTP, PwdFTP);

                clsRequest.Method = WebRequestMethods.Ftp.DeleteFile;

                using (var response = (FtpWebResponse)clsRequest.GetResponse())
                {
                    using (Stream datastream = response.GetResponseStream())
                    {
                        if (datastream == null)
                            return;
                        using (var sr = new StreamReader(datastream))
                        {
                            sr.ReadToEnd();
                            sr.Close();
                        }
                        datastream.Close();
                        response.Close();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        private  void DeleteFolder(string path)
        {


            try
            {

            
                _destinazione = Destinazione; 

                if (!(Destinazione.StartsWith("ftp://") || Destinazione.StartsWith("ftp:\\\\")))
                {
                    _destinazione = "ftp://" + Destinazione;  

                }

                var clsRequest = (FtpWebRequest)WebRequest.Create(_destinazione + "/" + path);
                clsRequest.Credentials = new NetworkCredential(UserFTP, PwdFTP);

                clsRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;

                using (var response = (FtpWebResponse)clsRequest.GetResponse())
                {
                    using (Stream datastream = response.GetResponseStream())
                    {
                        if (datastream == null)
                            return;
                        using (var sr = new StreamReader(datastream))
                        {
                            sr.ReadToEnd();
                            sr.Close();
                        }
                        datastream.Close();
                        response.Close();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
        public void RenameFile( string newFileName)
        {


            try
            {

            
                _destinazione = Destinazione; 

                if (!(Destinazione.StartsWith("ftp://") || Destinazione.StartsWith("ftp:\\\\")))
                {
                    _destinazione = "ftp://" + Destinazione; 

                }

                var clsRequest = (FtpWebRequest)WebRequest.Create(_destinazione + "/" + FileName);
                clsRequest.Credentials = new NetworkCredential(UserFTP, PwdFTP);
                clsRequest.KeepAlive = true;
                clsRequest.UseBinary = true;
                clsRequest.UsePassive = true;
                clsRequest.Method = WebRequestMethods.Ftp.Rename;
                clsRequest.RenameTo = newFileName;
                FtpWebResponse renameResponse = (FtpWebResponse)clsRequest.GetResponse();
                renameResponse.Close();

            }
            catch (Exception)
            {

                throw;
            }

        }

        public   List<FileObject> DirectoryListing(string path)
        {
            try
            {


                var regex = new Regex(@"^([d-])([rwxt-]{3}){3}\s+\d{1,}\s+.*?(\d{1,})\s+(\w+\s+\d{1,2}\s+(?:\d{4})?)(\d{1,2}:\d{2})?\s+(.+?)\s?$",
                    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

                _destinazione = Destinazione; 

                if (!(Destinazione.StartsWith("ftp://") || Destinazione.StartsWith("ftp:\\\\")))
                {
                    _destinazione = "ftp://" + Destinazione; 

                }
                var request = (FtpWebRequest)WebRequest.Create(_destinazione + "/" + path);
                request.Credentials = new NetworkCredential(UserFTP, PwdFTP);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                var result = new List<FileObject>();

                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null)
                            return null;
                        using (var reader = new StreamReader(responseStream))
                        {
                            try
                            {
                            bool eof=reader.EndOfStream;
                            }
                            catch 
                            {return null;}                              

                            while (!reader.EndOfStream)
                            {
                                string r = reader.ReadLine();
                                if (string.IsNullOrWhiteSpace(r))
                                    continue;
                                var reg = regex.Match(r);
                                var c = new FileObject
                                {
                                    FileName = reg.Groups[6].Value,
                                    IsDirectory = reg.Groups[1].Value.ToLower() == "d"
                                };
                                result.Add(c);
                            }
                            reader.Close();
                        }
                        response.Close();
                    }
                }

                return result;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public bool DirectoryExists(string directory)
        {
            bool directoryExists;
            _destinazione = Destinazione; 

            if (!(Destinazione.StartsWith("ftp://") || Destinazione.StartsWith("ftp:\\\\")))
            {
                _destinazione = "ftp://" + Destinazione; 

            }

            var request = (FtpWebRequest)WebRequest.Create(_destinazione + "/" + directory +"/");

            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(UserFTP, PwdFTP);

            try
            {
                using (request.GetResponse())
                {
                    directoryExists = true;
                }
            }
            catch (WebException)
            {
                directoryExists = false;
            }

            return directoryExists;
        }

    }
}
