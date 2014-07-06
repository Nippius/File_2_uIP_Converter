using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace File_2_uIP_Converter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       private String input_file_name, output_file_name, complete_input_file_name;
        private FileStream output_file;
        private String out_c_array_name;
        private byte[] internal_tmp_buf;
        private const int MAX_NUM_COLLUMNS = 10;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Input_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileInfo f_info = new FileInfo(ofd.FileName);
                out_c_array_name = complete_input_file_name = f_info.Name;
                out_c_array_name = out_c_array_name.Remove(out_c_array_name.Length - f_info.Extension.Length);  // Remove the file extension
                input_file_name = input_TB.Text = ofd.FileName;
            }
        }

        private void Output_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = ".c";

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                output_file_name = output_TB.Text = sfd.FileName;
            }
        }

        /// <summary>
        /// Converts the current output file name (inlcuding extension)
        /// to a byte array and appends it to the StringBuilder object.
        /// </summary>
        /// <param name="sb"> The StringBuilder object to appeed to.</param>
        private void AppendFileNameToArray(StringBuilder sb)
        {
            char[] tmp_c_arr = complete_input_file_name.ToCharArray();
            byte[] tmp_b_arr = new byte[tmp_c_arr.Length * sizeof(char)];
            System.Buffer.BlockCopy(tmp_c_arr, 0, tmp_b_arr, 0, tmp_b_arr.Length);

            String[] tmp_s_arr = BitConverter.ToString(tmp_b_arr, 0, tmp_b_arr.Length).Split('-');

            sb.Append("0x2f, ");

            foreach (String aux in tmp_s_arr)
            {
                if (aux.Equals("00"))
                {
                    continue; // Discard the unicode character. We only want ASCII
                }
                sb.Append("0x" + aux + ", ");
            }

            sb.Append("0x00,"); //Because C strings end with \0
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            if (input_file_name != null && output_file_name != null)
            {
                using (output_file = File.Open(output_file_name, FileMode.Create))  // If file exists, it´s overwriten!
                {
                    StringBuilder output_array = new StringBuilder();

                    // Append the array definition
                    output_array.Append("static const unsigned char " + out_c_array_name + "[] = {\n");

                    // Append the the array
                    AppendFileNameToArray(output_array);

                    internal_tmp_buf = File.ReadAllBytes(input_file_name);

                    // Convert bytes to their String representation
                    String[] temp = BitConverter.ToString(internal_tmp_buf, 0, internal_tmp_buf.Length).Split('-');

                    int counter = 0;
                    foreach (String aux in temp)
                    {
                        if (counter % MAX_NUM_COLLUMNS == 0) output_array.Append("\n");
                        output_array.Append("0x" + aux + ", ");
                        counter++;
                    }

                    output_array.Append("0x00 };");  //Because C strings end with \0

                    // Convert the output_array to a byte[]
                    // This is necessarie because FileStream.Write() receives a byte[]
                    internal_tmp_buf = new byte[output_array.Length * sizeof(char)];
                    System.Buffer.BlockCopy(output_array.ToString().ToCharArray(), 0, internal_tmp_buf, 0, internal_tmp_buf.Length);

                    // Write the array of byte to the output file.
                    output_file.Write(internal_tmp_buf, 0, internal_tmp_buf.Length);
                    result_LBL.Content = "Ok!";
                }
            }
            else if (input_file_name == null)
            {
                result_LBL.Content = "please select input file...";
            }
            else if (output_file_name == null)
            {
                result_LBL.Content = "please select output file...";
            }
        }
    }
}
