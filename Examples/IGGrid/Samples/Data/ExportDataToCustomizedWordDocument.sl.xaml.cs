﻿using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using IGGrid.Resources;
using Infragistics.Controls.Grids;
using Infragistics.Documents.Word;
using Infragistics.Samples.Framework;
using Infragistics.Samples.Shared.Controls;
using Infragistics.Samples.Shared.DataProviders;
using Infragistics.Samples.Shared.Extensions;
using Infragistics.Samples.Shared.Models;

namespace IGGrid.Samples.Data
{
    public partial class ExportDataToCustomizedWordDocument : SampleContainer
    {
        private XamGridWordWriter _wordWriter;
        private bool _isExportSuccessful = true;

        public ExportDataToCustomizedWordDocument()
        {
            InitializeComponent();
            this.Loaded += OnSampleLoaded;
        }

        void OnSampleLoaded(object sender, RoutedEventArgs e)
        {
            // Populate xamGrid with sample data
            DownloadDataSource();

            _wordWriter = this.Resources["wordWriter"] as XamGridWordWriter;

            Combo_Orientation.ItemsSource = EnumValuesProvider.GetEnumValues<PageOrientation>();
            Combo_Orientation.SelectedIndex = 0;
        }

        private void DownloadDataSource()
        {
            var _dataProvider = new XmlDataProvider();
            _dataProvider.GetXmlDataCompleted += OnDataProviderGetXmlDataCompleted;
            _dataProvider.GetXmlDataAsync("Products.xml");
        }

        void OnDataProviderGetXmlDataCompleted(object sender, GetXmlDataCompletedEventArgs e)
        {
            if (e.Error != null) return;

            XDocument doc = e.Result;
            var dataSource = (from d in doc.Descendants("Products")
                              select new Product
                              {
                                  SKU = d.Element("ProductID").GetString(),
                                  Name = d.Element("ProductName").GetString(),
                                  Category = d.Element("Category").GetString(),
                                  Supplier = d.Element("Supplier").GetString(),
                                  UnitPrice = d.Element("UnitPrice").GetDouble(),
                                  UnitsInStock = d.Element("UnitsInStock").GetInt(),
                                  UnitsOnOrder = d.Element("UnitsOnOrder").GetInt(),
                                  QuantityPerUnit = d.Element("QuantityPerUnit").GetString()
                              });

            this.xamGrid.ItemsSource = dataSource.ToList();
        }

        private void Btn_Export_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog { DefaultExt = "docx", Filter = "Word Document (.docx)|*.docx" };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    Stream stream = saveDialog.OpenFile();
                    using (stream)
                    {
                        // Export xamGrid data to a Word document
                        _wordWriter.Export(xamGrid, stream);
                        _isExportSuccessful = true;
                    }
                }
                catch (IOException exc)
                {
                    _isExportSuccessful = false;
                    MessageBox.Show(exc.Message);
                }
            }
        }

        private void XamGridWordWriter_ExportEnded(object sender, System.EventArgs e)
        {
            if (_isExportSuccessful)
            {
                MessageBox.Show(GridStrings.WordExport_Msg);
            }
        }

        private void Combo_Orientation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selectedItem = (PageOrientation)e.AddedItems[0];
                WordSectionSettings wss = this.Resources["wordSectionSettings"] as WordSectionSettings;
                wss.PageOrientation = selectedItem;
            }
        }
    }
}
