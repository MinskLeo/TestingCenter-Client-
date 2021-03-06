﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KursovayaYP
{
    public partial class TestsList : Form
    {

        private static int Port = 8888;
        private static string ID;
        private static string[] TEST;
        public static string TestName;//POIT_3_Math

        public TestsList(int port, string id)
        {
            InitializeComponent();
            //Тут 2 варианта развития событий:
            //1.Грузимся в блоке с конструктором - трабл с экзепшоном может быть, если не будет коннекта
            //2.Грузимся после загрузки данной формы - не совсем логично, пока попробуем второй вариант
            Port = port;
            ID = id;
        }

        private void TestsList_Load(object sender, EventArgs e)
        {
            try
            {
                TcpClient tcp=new TcpClient();
                tcp.Connect("127.0.0.1", Convert.ToInt32(Port));
                NetworkStream stream = tcp.GetStream();
                //ALL_DEBUG до слова ПРОВЕРИТЬ
                //StreamWriter NetWriter = new StreamWriter(stream);
                string request = "testslist_" + ID;

                //Я возможно понял что за дичь. Я походу запрос не отправлял... Лол кик чибурик
                stream.Write(Encoding.UTF8.GetBytes(request), 0, request.Length);

                while (!stream.DataAvailable)
                {
                    //Просто чтобы клиент подождал пока придет обработка с сервера
                }

                //ПРОВЕРИТЬ
                BinaryFormatter formatter = new BinaryFormatter();
                List<string> testslist = (List<string>)formatter.Deserialize(stream);
                //Тут мы немного подредактируем названия, что нам придут. Дабы не было путей к файлам)
                for(int i=0;i<testslist.Count;i++)
                {
                    //tests\POIT_3_Math.txt
                    //buf[0] buf[1] buf[2] buf[3] buf[4]
                    string[] buf = testslist[i].Split('\\','_','.');
                    testslist[i] = buf[1] + "_" + buf[2] + "_" + buf[3];
                }
                list_Tests.Items.AddRange(testslist.ToArray());

                //Закрываем потоки
                //NetWriter.Close();
                stream.Close();
                tcp.Close();
                list_Tests.SelectedIndex = 0;
            }
            catch(SocketException ex)
            {
                MessageBox.Show("Ошибка подключения. Сервер не отвечает.\n" + ex.Message + "\n" + ex.StackTrace, "Подключение", MessageBoxButtons.OK, MessageBoxIcon.Error);//DEBUG
            }
        }

        private void but_Choose_Click(object sender, EventArgs e)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect("127.0.0.1", Port);
                NetworkStream stream = client.GetStream();
                byte[] message = Encoding.UTF8.GetBytes("test_"+list_Tests.SelectedItem.ToString());
                TestName = list_Tests.SelectedItem.ToString();//Название нашего теста
                stream.Write(message,0,message.Length);
                //Тут надо заресивить сам тест
                while(!stream.DataAvailable)
                {
                    //Пождемс
                }

                BinaryFormatter formatter = new BinaryFormatter();
                TEST = ((string[])formatter.Deserialize(stream));
                stream.Close();
                client.Close();
                //Работаем с полученным материалом (от фокус группы)
                if(TEST.Length!=0 || TEST!=null)
                {
                    Test test = new Test(ID, TEST, Port);
                    test.Disposed += new EventHandler(IfClosed);
                    this.Hide();
                    test.Show();
                }
                else
                {
                    if(MessageBox.Show("Некорректный тест!","Тест", MessageBoxButtons.RetryCancel,MessageBoxIcon.Error)==DialogResult.Retry)
                    {
                        but_Choose_Click(sender, e);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch(SocketException ex)
            {
                MessageBox.Show("Ошибка подключения. Сервер не отвечает.\n" + ex.Message + "\n" + ex.StackTrace, "Подключение", MessageBoxButtons.OK, MessageBoxIcon.Error);//DEBUG
            }
        }

        private void IfClosed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void but_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
