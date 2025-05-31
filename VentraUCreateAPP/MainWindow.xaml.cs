using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Xml.Linq;

namespace VentraUCreateAPP
{
    public sealed partial class MainWindow : Window
    {
        private string projectPath = @"D:\dks"; // Путь к проекту
        private string msbuildPath = @"D:\vstudio\MSBuild\Current\Bin\MSBuild.exe"; // Путь к MSBuild.exe
        private bool isPlacingButton = false;
        private bool isPlacingComboBox = false;
        private bool isPlacingAButton = false;
        private bool isPlacingTButton = false;
        private bool isPlacingSplitterButton = false;
        private bool isPlacingASplitterButton = false;
        private bool isPlacingCheckBox = false;
        private bool isPlacingRadioButton = false;
        private Control selectedElement = null;


        public MainWindow()
        {
            this.InitializeComponent();
            ElementSoundPlayer.State = ElementSoundPlayerState.Off;
            ElementSoundPlayer.State = ElementSoundPlayerState.On;

        }

        private void SelectElement(Control element)
        {
            // Убираем выделение с предыдущего элемента
            if (selectedElement != null)
            {
                Title = "Без имени - VentraUCreateAPP";
            }

            // Устанавливаем новое выделение
            selectedElement = element;
            Title = "Активный элемент - VentraUCreateAPP";


            // Получаем текущую позицию элемента на Canvas
            double left = Canvas.GetLeft(element);
            double top = Canvas.GetTop(element);

            // Устанавливаем значения координат в элементы управления
            textbox2.Value = left;
            textbox3.Value = top;

            // Устанавливаем ширину и высоту
            wid.Value = selectedElement.Width;
            heid.Value = selectedElement.Height;

            // Устанавливаем текст в textbox1 в зависимости от типа контрола
            if (selectedElement is ContentControl contentControl)
            {
                textbox1.Text = contentControl.Content?.ToString();
            }
            else if (selectedElement is TextBox tb)
            {
                textbox1.Text = tb.Text;
            }
            else if (selectedElement is ComboBox comboBox)
            {
                textbox1.Text = comboBox.Items.Count > 0 ? comboBox.Items[0]?.ToString() : "";
            }
            else if (selectedElement is CheckBox cb)
            {
                textbox1.Text = cb.Content?.ToString();
            }
            else if (selectedElement is RadioButton rb)
            {
                textbox1.Text = rb.Content?.ToString();
            }
        }


        public void Stopall()
        {
            Title = "Без имени - VentraUCreateAPP";
            isPlacingButton = false;
            isPlacingComboBox = false; // отключаем другие режимы
            isPlacingAButton = false;
            isPlacingTButton = false;
            isPlacingCheckBox = false;
            isPlacingSplitterButton = false;
            isPlacingASplitterButton = false;
            isPlacingRadioButton = false;


        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Stopall();

            isPlacingButton = true;
        }


        private void AnimateElement(UIElement element)
        {
            var visual = ElementCompositionPreview.GetElementVisual(element);
            var compositor = visual.Compositor;

            // Начальные параметры (прозрачный и нулевой размер)
            visual.Opacity = 0;
            visual.Scale = new System.Numerics.Vector3(0.3f, 0.3f, 1f);

            // Анимация прозрачности
            var opacityAnim = compositor.CreateScalarKeyFrameAnimation();
            opacityAnim.InsertKeyFrame(1f, 1f);
            opacityAnim.Duration = TimeSpan.FromMilliseconds(300);

            // Анимация масштаба
            var scaleAnim = compositor.CreateVector3KeyFrameAnimation();
            scaleAnim.InsertKeyFrame(1f, new System.Numerics.Vector3(1f, 1f, 1f));
            scaleAnim.Duration = TimeSpan.FromMilliseconds(300);

            // Устанавливаем центр масштабирования
            visual.CenterPoint = new System.Numerics.Vector3((float)(element.RenderSize.Width / 2), (float)(element.RenderSize.Height / 2), 0);

            // Запуск анимаций
            visual.StartAnimation("Opacity", opacityAnim);
            visual.StartAnimation("Scale", scaleAnim);
        }



        private void DesignCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

            var point = e.GetCurrentPoint(DesignCanvas).Position;


            if (isPlacingButton)
            {
                Button newButton = new Button();
                newButton.Content = textbox1.Text;
                newButton.Width = wid.Value;
                newButton.Height = heid.Value;
                newButton.Tapped += (s, ev) => SelectElement(newButton);


                Canvas.SetLeft(newButton, point.X);
                Canvas.SetTop(newButton, point.Y);

                DesignCanvas.Children.Add(newButton);
                AnimateElement(newButton); // ? добавь анимацию здесь
                Stopall();

            }
            else if (isPlacingAButton)
            {
                Button newaButton = new Button();
                newaButton.Content = textbox1.Text;
                newaButton.Width = wid.Value;
                newaButton.Style = run.Style;
                newaButton.Height = heid.Value;
                newaButton.Tapped += (s, ev) => SelectElement(newaButton);

                Canvas.SetLeft(newaButton, point.X);
                Canvas.SetTop(newaButton, point.Y);

                DesignCanvas.Children.Add(newaButton);
                AnimateElement(newaButton); // ? добавь анимацию здесь
                Stopall();

            }
            else if (isPlacingTButton)
            {
                ToggleButton newTButton = new ToggleButton();
                newTButton.Content = textbox1.Text;
                newTButton.Width = wid.Value;
                newTButton.Height = heid.Value;
                newTButton.Tapped += (s, ev) => SelectElement(newTButton);

                Canvas.SetLeft(newTButton, point.X);
                Canvas.SetTop(newTButton, point.Y);

                DesignCanvas.Children.Add(newTButton);
                AnimateElement(newTButton); // ? добавь анимацию здесь
                Stopall();

            }


            else if (isPlacingASplitterButton)
            {

                ToggleSplitButton newASPButton = new ToggleSplitButton();
                newASPButton.Content = textbox1.Text;
                newASPButton.Width = wid.Value;
                newASPButton.Height = heid.Value;
                newASPButton.Tapped += (s, ev) => SelectElement(newASPButton);

                Canvas.SetLeft(newASPButton, point.X);
                Canvas.SetTop(newASPButton, point.Y);

                DesignCanvas.Children.Add(newASPButton);
                AnimateElement(newASPButton); // ? добавь анимацию здесь
                Stopall();
            }

            else if (isPlacingSplitterButton)
            {

                SplitButton newSPButton = new SplitButton();
                newSPButton.Content = textbox1.Text;
                newSPButton.Width = wid.Value;
                newSPButton.Height = heid.Value;
                newSPButton.Tapped += (s, ev) => SelectElement(newSPButton);

                Canvas.SetLeft(newSPButton, point.X);
                Canvas.SetTop(newSPButton, point.Y);

                DesignCanvas.Children.Add(newSPButton);
                AnimateElement(newSPButton); // ? добавь анимацию здесь
                Stopall();
            }
            else if (isPlacingRadioButton)
            {
                RadioButton newRadioButton = new RadioButton();
                newRadioButton.Content = textbox1.Text;
                newRadioButton.Width = wid.Value;
                newRadioButton.Tapped += (s, ev) => SelectElement(newRadioButton);

                Canvas.SetLeft(newRadioButton, point.X);
                Canvas.SetTop(newRadioButton, point.Y);

                DesignCanvas.Children.Add(newRadioButton);
                AnimateElement(newRadioButton); // ? добавь анимацию здесь
                Stopall();
            }
            else if (isPlacingComboBox)
            {

                ComboBox newComboBox = new ComboBox();
                newComboBox.Width = wid.Value;
                newComboBox.Height = heid.Value;
                newComboBox.Items.Add("Выбор 1");
                newComboBox.Items.Add("Выбор 2");
                newComboBox.Items.Add("Выбор 3");
                newComboBox.Tapped += (s, ev) => SelectElement(newComboBox);

                Canvas.SetLeft(newComboBox, point.X);
                Canvas.SetTop(newComboBox, point.Y);

                DesignCanvas.Children.Add(newComboBox);
                AnimateElement(newComboBox); // ? добавь анимацию здесь
                Stopall();
            }

            else if (isPlacingCheckBox)
            {

                CheckBox newCheckBox = new CheckBox();
                newCheckBox.Content = textbox1.Text;
                newCheckBox.Width = wid.Value;
                newCheckBox.Height = heid.Value;
                newCheckBox.Tapped += (s, ev) => SelectElement(newCheckBox);

                Canvas.SetLeft(newCheckBox, point.X);
                Canvas.SetTop(newCheckBox, point.Y);

                DesignCanvas.Children.Add(newCheckBox);
                AnimateElement(newCheckBox); // ? добавь анимацию здесь
                Stopall();


            }
            if (e.OriginalSource == DesignCanvas)
            {
                selectedElement = null;
                Title = "Без имени - VentraUCreateAPP";

            }
        }

        private void AddAButton_Click(object sender, RoutedEventArgs e)
        {
            Stopall();

            isPlacingAButton = true;
        }

        private void AddSplitterButton_Click(object sender, RoutedEventArgs e)
        {
            Stopall();

            isPlacingSplitterButton = true;
        }

        private void AddASplitterButton_Click(object sender, RoutedEventArgs e)
        {
            Stopall();

            isPlacingASplitterButton = true;
        }

        private void AddToggleButton_Click(object sender, RoutedEventArgs e)
        {
            Stopall();

            isPlacingTButton = true;
        }

        // Добавление кнопки в Canvas
        private void AddTextBox_Click(object sender, RoutedEventArgs e)
        {
            TextBox newTextBox = new TextBox();
            newTextBox.PlaceholderText = textbox1.Text;
            newTextBox.Width = wid.Value;
            newTextBox.Height = heid.Value;

            // Размещение в случайной позиции
            double x = new Random().Next(10, 300);
            double y = new Random().Next(10, 200);
            Canvas.SetLeft(newTextBox, x);
            Canvas.SetTop(newTextBox, y);

            DesignCanvas.Children.Add(newTextBox);
            AnimateElement(newTextBox); // ? добавь анимацию здесь
        }

        // Добавление кнопки в Canvas
        private void AddCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Stopall();

            isPlacingCheckBox = true;
        }

        // Запуск компиляции проекта
        private void CompileApp_Click(object sender, RoutedEventArgs e)
        {
            string solutionFile = Path.Combine(projectPath, "myexampleapp.sln");

            // Проверяем существование файла
            if (!File.Exists(solutionFile))
            {
                Debug.WriteLine($"Ошибка: Файл {solutionFile} не найден.");
                return;
            }

            // Настраиваем вызов компилятора
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = msbuildPath,
                Arguments = $"\"{solutionFile}\" /p:Configuration=Release",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Запускаем процесс
            Process process = new Process { StartInfo = psi };
            Debug.WriteLine("Запуск MSBuild...");
            process.Start();

            // Читаем вывод и ошибки
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            // Логируем результат
            Debug.WriteLine("Компиляция завершена.");
            Debug.WriteLine("Вывод: " + output);
            Debug.WriteLine("Ошибка: " + error);
        }


        private void AddPasswordBox_Click(object sender, RoutedEventArgs e)
        {
            PasswordBox newPasswordBox = new PasswordBox();
            newPasswordBox.Width = wid.Value;
            newPasswordBox.Height = heid.Value;


            double x = new Random().Next(10, 300);
            double y = new Random().Next(10, 200);
            Canvas.SetLeft(newPasswordBox, x);
            Canvas.SetTop(newPasswordBox, y);

            DesignCanvas.Children.Add(newPasswordBox);
            AnimateElement(newPasswordBox); // ? добавь анимацию здесь
        }

        private void AddSlider_Click(object sender, RoutedEventArgs e)
        {
            Slider newSlider = new Slider();
            newSlider.Minimum = 0;
            newSlider.Maximum = 100;
            newSlider.Width = wid.Value;

            double x = new Random().Next(10, 300);
            double y = new Random().Next(10, 200);
            Canvas.SetLeft(newSlider, x);
            Canvas.SetTop(newSlider, y);

            DesignCanvas.Children.Add(newSlider);
            AnimateElement(newSlider);
        }

        private void AddProgressBar_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar newProgressBar = new ProgressBar();
            newProgressBar.Minimum = 0;
            newProgressBar.Maximum = 100;
            newProgressBar.Value = 50;
            newProgressBar.Width = wid.Value;
            newProgressBar.Height = heid.Value;

            double x = new Random().Next(10, 300);
            double y = new Random().Next(10, 200);
            Canvas.SetLeft(newProgressBar, x);
            Canvas.SetTop(newProgressBar, y);

            DesignCanvas.Children.Add(newProgressBar);
            AnimateElement(newProgressBar); // ? добавь анимацию здесь
        }

        private void AddComboBox_Click(object sender, RoutedEventArgs e)
        {
            Stopall();

            isPlacingComboBox = true; // отключаем другие режимы
        }

        private void Textbox2_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (selectedElement != null)
            {
                double x = textbox2.Value;
                double y = Canvas.GetTop(selectedElement);
                Canvas.SetLeft(selectedElement, x);
            }
        }

        private void Textbox3_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (selectedElement != null)
            {
                double x = Canvas.GetLeft(selectedElement);
                double y = textbox3.Value;
                Canvas.SetTop(selectedElement, y);
            }
        }

        private void Textbox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedElement != null)
            {
                string newText = textbox1.Text;

                if (selectedElement is ContentControl contentControl)
                {
                    contentControl.Content = newText;
                }
                else if (selectedElement is TextBox tb)
                {
                    tb.Text = newText;
                }
                else if (selectedElement is ComboBox comboBox)
                {
                    if (comboBox.Items.Count > 0)
                    {
                        comboBox.Items[0] = newText;
                    }
                    else
                    {
                        comboBox.Items.Add(newText);
                    }
                }
                else if (selectedElement is CheckBox cb)
                {
                    cb.Content = newText;
                }
                else if (selectedElement is RadioButton rb)
                {
                    rb.Content = newText;
                }
            }
        }


        private void Wid_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (selectedElement != null)
            {
                selectedElement.Width = e.NewValue;
            }
        }

        private void Heid_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (selectedElement != null)
            {
                selectedElement.Height = e.NewValue;
            }
        }



        private void DeleteSelectedElement()
        {
            if (selectedElement != null)
            {
                DesignCanvas.Children.Remove(selectedElement);
                selectedElement = null;
                Title = "Без имени - VentraUCreateAPP";
                Stopall();
            }
        }


        private void DeleteElement_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedElement();
        }




        private void AddRadioButton_Click(object sender, RoutedEventArgs e)
        {
            Stopall();

            isPlacingRadioButton = true;

        }


    }
}