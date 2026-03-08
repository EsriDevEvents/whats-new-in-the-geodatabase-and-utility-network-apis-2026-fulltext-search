using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace FullTextSearchDevSummitDemo
{
  public class DatasetField
  {
    public bool IsSelected { get; set; }

    private string _fieldName = "";
    public string FieldName
    {
      get { return _fieldName; }
      set { _fieldName = value; }
    }
  }

  public class SearchDockPaneViewModel : DockPane
  {
    private const string _dockPaneID = "FullTextSearchDevSummitDemo_SearchDockPane";
    private ObservableCollection<string> _datasets = new ObservableCollection<string>();
    private ObservableCollection<DatasetField> _datasetFields = new ObservableCollection<DatasetField>();
    private IEnumerable<MapMember> _mapMembers = null;



    protected SearchDockPaneViewModel()
    {
      _mapMembers = MapView.Active.Map.GetMapMembersAsFlattenedList();
      ReadDatasets();

    }
    private void ReadDatasets()
    {
      IEnumerable<string> layers = _mapMembers.Select(l => l.Name);
      _datasets = new ObservableCollection<string>(layers);
    }

    private ICommand _selectionCommand = null;
    public ICommand SelectionCommand
    {
      get
      {
        if (_selectionCommand == null)
        {
          _selectionCommand = new RelayCommand(ExecuteSearch);
        }
        return _selectionCommand;
      }
    }


    private ICommand _searchCommand = null;
    public ICommand SearchCommand
    {
      get
      {
        if (_searchCommand == null)
        {
          _searchCommand = new RelayCommand(ExecuteSearch);
        }
        return _searchCommand;
      }
    }
    private int? _selectedRadio = null;
    public int? SelectedRadio
    {
      get { return _selectedRadio; }
      set
      {
        if (_selectedRadio != value)
        {
          _selectedRadio = value;
          SetProperty(ref _selectedRadio, value, () => SelectedRadio);
        }
      }
    }

    public ObservableCollection<string> Datasets
    {
      get
      {
        return _datasets;
      }
      set
      {
        SetProperty(ref _datasets, value, () => Datasets);
      }
    }

    public ObservableCollection<DatasetField> DatasetFields
    {
      get
      {
        return _datasetFields;
      }
      set
      {
        SetProperty(ref _datasetFields, value, () => DatasetFields);
      }
    }


    private string _selectedDataset;
    public string SelectedDataset
    {
      get => _selectedDataset;
      set
      {
        SetProperty(ref _selectedDataset, value, () => SelectedDataset);
        HandleDatasetSelection(_selectedDataset);
      }
    }

    private string _selectedField;
    public string SelectedField
    {
      get => _selectedField;
      set
      {
        SetProperty(ref _selectedField, value, () => SelectedField);
      }
    }

    private string _searchTerm = null;
    public string SearchTerm
    {
      get => _searchTerm;
      set
      {
        SetProperty(ref _searchTerm, value, () => SearchTerm);
      }
    }

    private string _searchExpression = null;
    public string SearchExpression
    {
      get => _searchExpression;
      set
      {
        SetProperty(ref _searchExpression, value, () => SearchExpression);
      }
    }

    private void HandleDatasetSelection(string selectedDataset)
    {
      QueuedTask.Run(() =>
      {
        IEnumerable<DatasetField> datasetFields = new List<DatasetField>();
        IEnumerable<string> fieldNames = new List<string>();

        foreach (var item in _mapMembers)
        {
          if (item.Name == selectedDataset)
          {
            if (item.GetType() == typeof(FeatureLayer))
            {
              FeatureLayer featureLayer = _mapMembers.First(m => m.GetType() == typeof(FeatureLayer)) as FeatureLayer;
              fieldNames = featureLayer.GetFeatureClass().GetDefinition().GetFields().Select(f => f.Name);

              foreach (var fieldName in fieldNames)
              {
                datasetFields = datasetFields.Append(new DatasetField() { FieldName = fieldName, IsSelected = false });
              }
            }
            else if (item.GetType() == typeof(StandaloneTable))
            {
              StandaloneTable standaloneTable = _mapMembers.First(m => m.GetType() == typeof(StandaloneTable)) as StandaloneTable;
              fieldNames = standaloneTable.GetTable().GetDefinition().GetFields().Select(f => f.Name);
              foreach (var fieldName in fieldNames)
              {
                datasetFields = datasetFields.Append(new DatasetField() { FieldName = fieldName, IsSelected = false });
              }
            }
          }
        }
        DatasetFields = new ObservableCollection<DatasetField>(datasetFields);
      });
    }

    private void ExecuteSearch()
    {
      string searchTerm = SearchTerm;
      string searchExpression = SearchExpression;
      int? radio = SelectedRadio;

      // Selected fields for search
      List<string> searchFields = DatasetFields.Where(f => f.IsSelected.Equals(true)).Select(f => f.FieldName).ToList();
      string searchFieldsAsString = string.Join(", ", searchFields);


      // TODO:
      FullTextSearchTermExpression fullTextSearchTermExpression1 = new FullTextSearchTermExpression()
      {
        SearchTerm = searchTerm,
        SearchFields = searchFieldsAsString,
        SearchType = FullTextSearchType.Simple
      };

      FullTextExpression fullTextSearchTermExpression2 = new FullTextSearchTermExpression()
      {
        SearchTerm = searchExpression,
        SearchFields = searchFieldsAsString,
        SearchType = FullTextSearchType.Simple
      };

      QueryFilter queryFilter = new QueryFilter();

      switch (radio)
      {
        case 1:
          {
            // OR
            FullTextOrExpression orExpression = new FullTextOrExpression(fullTextSearchTermExpression1, fullTextSearchTermExpression2);
            queryFilter.FullTextExpression = orExpression;
            break;
          }

        case 2:
          {
            // AND
            FullTextAndExpression andExpression = new FullTextAndExpression(fullTextSearchTermExpression1, fullTextSearchTermExpression2);
            queryFilter.FullTextExpression = andExpression;
            break;
          }
        case 3:
          {
            // WHERE
            FullTextSqlExpression fullTextSqlExpression = new FullTextSqlExpression()
            {
              WhereClause = "Description IS NOT NULL"
            };
            queryFilter.FullTextExpression = fullTextSqlExpression;

            break;
          }
        case null:
          // Default
          queryFilter.FullTextExpression = fullTextSearchTermExpression1;
          break;
      }


      QueuedTask.Run(() =>
      {
        foreach (var item in _mapMembers)
        {
          if (item.Name == SelectedDataset)
          {
            if (item.GetType() == typeof(FeatureLayer))
            {
              FeatureLayer featureLayer = _mapMembers.First(m => m.GetType() == typeof(FeatureLayer)) as FeatureLayer;
              if (!CheckDataStoreSupportsFullext(featureLayer.GetFeatureClass()) && !CheckDatasetHasFullextIndex<TableDefinition>(featureLayer.GetFeatureClass().GetDefinition()))
              {
                return;
              }
              using (Selection selection = featureLayer.Select(queryFilter, SelectionCombinationMethod.New))
              {
                var selectcount = selection.GetCount();
              }

            }
            else if (item.GetType() == typeof(StandaloneTable))
            {
              StandaloneTable standaloneTable = _mapMembers.First(m => m.GetType() == typeof(StandaloneTable)) as StandaloneTable;
              if (!CheckDataStoreSupportsFullext(standaloneTable.GetTable()) && !CheckDatasetHasFullextIndex<TableDefinition>(standaloneTable.GetTable().GetDefinition()))
              {
                return;
              }
              using (Selection selection = standaloneTable.Select(queryFilter, SelectionCombinationMethod.New))
              {
                var selectcount = selection.GetCount();
              }
            }
          }
        }
      });

      #region Check for full-text index support 
      // Check if the datastore supports full-text index
      bool CheckDataStoreSupportsFullext(Table table)
      {
        using (var dataStore = table.GetDatastore())
        {
          return dataStore.GetDatastoreProperties().SupportsFullTextIndex;
        }
      }

      // Check if the dataset has a full-text index defined
      bool CheckDatasetHasFullextIndex<T>(T definition)
      {
        if (definition is TableDefinition tableDefinition)
        {
          return tableDefinition.GetIndexes().Any(i => i.IsFullText());
        }
        else if (definition is FeatureClassDefinition featureClassDefinition)
        {
          return featureClassDefinition.GetIndexes().Any(i => i.IsFullText());
        }
        return false;
      }
      #endregion Check for full-text index support
    }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
  public class SearchDockPane_ShowButton : Button
  {
    protected override void OnClick()
    {
      SearchDockPaneViewModel.Show();
    }
  }

}
