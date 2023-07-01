using Bone_Fracture_Detection.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Bone_Fracture_Detection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                // Get the selected file path
                string originalImagePath = openFileDialog.FileName;

                string destinationDirectory = "D:\\FYP\\image_to_predict\\image1.png";
                System.IO.File.Copy(originalImagePath, destinationDirectory,true);

                // Load the image into a BitmapImage
                BitmapImage bitmapImage = new BitmapImage(new Uri(originalImagePath));

                // Set the image source of an Image control named "myImage" to the selected image
                imageShow.Source = bitmapImage;
            }
        }

        private void checkButton_Click(object sender, RoutedEventArgs e)
        {
            var firstname = firstnameInput;
            var lastname = lastnameInput;
            var email = emailInput;

            if (!string.IsNullOrEmpty(firstname.Text) && !string.IsNullOrEmpty(lastname.Text) && !string.IsNullOrEmpty(email.Text) && isEmailValid(email.Text) && imageShow.Source != null)
            {
                firstname.BorderBrush = Brushes.Gray;
                lastname.BorderBrush = Brushes.Gray;
                email.BorderBrush = Brushes.Gray;

                var helper = new Helper();
                var isSuccess = helper.runBatFile();

                if (isSuccess.ToLower() == "success")
                {
                    string predictionResultFilePath = "D:\\FYP\\prediction_result.txt";

                    Dictionary<string,string> predictions = new Dictionary<string, string>();


                    using (StreamReader sr = new StreamReader(predictionResultFilePath))
                    {
                        // Loop through each line in the text file and read its contents
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            var res = line.Split(":")[1];
                            res = res.TrimStart();
                            res=res.TrimEnd();

                            if (line.ToUpper().Contains("BINARY"))
                            {
                                predictions.Add("BINARY", res);
                            }
                            else if (line.ToUpper().Contains("CLASS"))
                            {
                                predictions.Add("CLASS", res);
                            }
                            else if (line.ToUpper().Contains("BOXES"))
                            {
                                predictions.Add("BOXES", res);
                            }
                        }

                        predictions = helper.healthAdviceDescription(predictions);

                        helper.fillReportInformations(predictions,firstname.Text,lastname.Text);
                        var isSent = helper.sendEmail(email.Text);
                        if (isSent)
                        {
                            MessageBox.Show("Email sent successfully", "Fracture");
                            imageShow.Source = null;
                        }
                    }

                }

            }
            else
            {
                if (string.IsNullOrEmpty(firstname.Text))
                {
                    firstname.BorderBrush = Brushes.Red;
                }
                else if (string.IsNullOrEmpty(lastname.Text))
                {
                    lastname.BorderBrush = Brushes.Red;
                }
                else if(string.IsNullOrEmpty(email.Text) || !isEmailValid(email.Text))
                {
                    email.BorderBrush = Brushes.Red;
                }else if(imageShow.Source == null)
                {
                    MessageBox.Show("Please load an image", "Load Image");
                }
            }
        }


        private bool isEmailValid(string email)
        {
            try
            {
                MailAddress mailAddress = new MailAddress(email);
                // If no exception was thrown, the email address is valid
                return true;
            }
            catch (FormatException)
            {
                // If a FormatException was thrown, the email address is invalid
                return false;
            }
        }

        private void firstnameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            if (!string.IsNullOrEmpty(firstnameInput.Text))
            {
                firstnameInput.BorderBrush = Brushes.Gray;
            }
            else
            {
                firstnameInput.BorderBrush = Brushes.Red;
            }
        }

        private void lastnameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(lastnameInput.Text))
            {
                lastnameInput.BorderBrush = Brushes.Gray;
            }
            else
            {
                lastnameInput.BorderBrush = Brushes.Red;
            }
        }

        private void emailInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(emailInput.Text))
            {
                emailInput.BorderBrush = Brushes.Gray;
            }
            else
            {
                emailInput.BorderBrush = Brushes.Red;
            }
        }
    }
}
