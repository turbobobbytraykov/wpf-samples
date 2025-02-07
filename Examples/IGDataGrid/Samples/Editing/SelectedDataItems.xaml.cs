﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Samples.Framework;
using Infragistics.Samples.Shared.DataProviders;
using Infragistics.Samples.Shared.Extensions;
using Infragistics.Samples.Shared.Models;

namespace IGDataGrid.Samples.Editing
{
    public partial class SelectedDataItems : SampleContainer
    {
        private XmlDataProvider xmlDataProvider;

        public SelectedDataItems()
        {
            InitializeComponent();
            DownloadDataSource();
        }

        /// <summary>
        /// This method is used for downloading the sample's data source. To support localization we created a custom helper class LocalizationHelper. 
        /// To use the sample code outside of the sample browser remove LocalizationHelper.GetLocalizeDataSource and create a URI directly.
        /// </summary>
        private void DownloadDataSource()
        {
            // create xml data provider that will load data from xml file
            xmlDataProvider = new XmlDataProvider();
            xmlDataProvider.GetXmlDataCompleted += OnDataProviderGetXmlDataCompleted;
            xmlDataProvider.GetXmlDataAsync("Movies.xml"); // xml file name
        }

        /// <summary>
        /// This method creates a collection of the DataItems objects by parsing the XML content and set it as ItemsSource.
        /// </summary>
        void OnDataProviderGetXmlDataCompleted(object sender, GetXmlDataCompletedEventArgs e)
        {
            DataSet ds = new DataSet();

            string fileContent = e.Result.ToString();
            using (TextReader reader = new StringReader(fileContent))
            {
                ds.ReadXml(reader);

                // Set the DataSource to null before setting it to the new source to work  
                // around a refresh issue with AddRecords when changing the DataSource.
                this.XamDataGrid1.DataSource = null;

                this.XamDataGrid1.DataSource = ds.Tables[0].DefaultView;
            }
        }

    }
}
