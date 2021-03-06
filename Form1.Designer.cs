﻿using System.Threading;
using NetMQ.Sockets;
using NetMQ;
using System.Windows.Forms;
using OpenCvSharp;
using Newtonsoft.Json;
using System;

namespace cnn_zmq_dotnet_client
{
    public class task
    {
        public int a {get; set;}
        public int b {get; set;}
        public Mat image {get; set;}
    } 
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        RequestSocket client;
        
        PictureBox pic = new PictureBox();
        private void InitializeComponent()
        {
            
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            var panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            this.Controls.Add(panel);
            this.Text = "Form1";
            var kill_btn = new Button();
            kill_btn.Text = "kill service";
            kill_btn.Click += (o, p) => {
                Thread request = new Thread(new ThreadStart(()=>{
                    try{
                        
                        ServiceTask kill_task = new ServiceTask();
                        kill_task.command = "kill";
                        var json_str = image_json_converter.serialaze(kill_task);
                        string message;


                        if (client.TrySendFrame(System.TimeSpan.FromSeconds(2), json_str) && 
                            client.TryReceiveFrameString(System.TimeSpan.FromSeconds(2), out message))
                        {
                            System.Console.WriteLine("server resp: {0}", message);
                        }else{
                            System.Console.WriteLine("server is down");
                            client.Disconnect("tcp://localhost:5555");
                            client.Dispose();
                            client = new RequestSocket();
                            client.Connect("tcp://localhost:5555");
                        }
                    }catch(System.Exception e){
                        System.Console.WriteLine("didnt send, {0}", e.Message);
                    }
                }));
                request.IsBackground = true;
                request.Start();           
            };
            panel.Controls.Add(kill_btn);

            var sendimg_btn = new Button();
            sendimg_btn.Text = "send img";
            sendimg_btn.Click += (o, p) => {
                Thread request = new Thread(new ThreadStart(()=>{
            
                        
                    Mat img = Cv2.ImRead("../cnn_zmq_service/test.jpg");

                    CNNTask task = new CNNTask();
                    task.a = 10;
                    task.b = 11;
                    task.image = img;

                    
                    var json_str = image_json_converter.serialaze(task);

                    var json_obj = image_json_converter.deserialaze(json_str);
                    if (Object.ReferenceEquals(json_obj.GetType(), typeof(CNNTask)))
                    {
                        CNNTask cnn_tast = json_obj as CNNTask;

                        pic.Invoke((MethodInvoker)(()=>{
                            pic.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(cnn_tast.image);
                        }));
                        
                    }
                    
                    string message;
                    if (client.TrySendFrame(System.TimeSpan.FromSeconds(2), json_str) && 
                        client.TryReceiveFrameString(System.TimeSpan.FromSeconds(2), out message))
                    {
                        var recv_json_obj = image_json_converter.deserialaze(message);
                        if (Object.ReferenceEquals(recv_json_obj.GetType(), typeof(CNNAnswer)))
                        {
                            CNNAnswer cnn_answer = recv_json_obj as CNNAnswer;

                            System.Console.WriteLine("res: {0}", cnn_answer.res);
                            pic.Invoke((MethodInvoker)(()=>{
                                pic.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(cnn_answer.image);
                            }));
                            
                        }
                    }else{
                        System.Console.WriteLine("server is down");
                        client.Disconnect("tcp://localhost:5555");
                        client.Dispose();
                        client = new RequestSocket();
                        client.Connect("tcp://localhost:5555");
                    }
                }));
                request.IsBackground = true;
                request.Start();           
            };
            panel.Controls.Add(sendimg_btn);


            pic.Size = new System.Drawing.Size(200, 400);
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            panel.Controls.Add(pic);
            

            client = new RequestSocket();
            client.Connect("tcp://localhost:5555");

            
        }

        #endregion
    }
}

