using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using RedConn;
using RedEquation.Classes;
using RedEquation.Classes.Enums;
using RedEquation.Helpers;
using MessageBox = System.Windows.MessageBox;
using Object = System.Object;
using TreeView = System.Windows.Forms.TreeView;
using UserControl = System.Windows.Controls.UserControl;

namespace RedEquation.Controls
{
    public partial class ProjectExplorerControl : UserControl
    {
        private Main _parent;

        private TreeView _winFormsTreeView;

        private RemoteConn _remoteConnection;
        private RemoteObj _currentProject;
        private RemoteObj _currentObject;
        private RemoteParam _currentParameter;
        private String _currentUserName;
        private Boolean _isUserAuthorized;

        internal RemoteConn RemoteConnection {
            get { return _remoteConnection;}
            set { _remoteConnection = value; }
        }
        internal RemoteObj CurrentObject {get{return _currentObject;}}


        #region Constructor And Initializing Methods
        
        public ProjectExplorerControl(Main parent)
        {
            _parent = parent;
            InitializeComponent();
            AddWinFormsTreeView();
            ShowLoadingProcess();
        }

        private void AddWinFormsTreeView()
        {
            WindowsFormsHost host = new WindowsFormsHost();
            _winFormsTreeView = new TreeView();
            _winFormsTreeView.AfterSelect += _winFormsTreeView_AfterSelect;
            _winFormsTreeView.Dock = DockStyle.Fill;
            host.Child = _winFormsTreeView;
            WinFormsTreeViewHolder.Children.Add(host);
        }

        #endregion



        #region User Panel Methods

        private void LogOut()
        {
            if (_remoteConnection == null) return;
            _remoteConnection.UserLogout();
            CheckIfUserIsAuthorized();
        }

        private void LogIn()
        {
            if (_remoteConnection == null) return;
            _remoteConnection.UserLogin();
            CheckIfUserIsAuthorized();
            if (_isUserAuthorized)
                RefreshControl();
        }

        #endregion



        #region Project Panel Methods

        private void FillProjectsComboBox(RemoteObj projectToSelect = null)
        {
            ClearComboBox(ProjectsComboBox);
            if (_remoteConnection == null) return;

            RemoteObj projects = _remoteConnection.GetProjects();

            ProjectsComboBox.DisplayMemberPath = "Text";
            ProjectsComboBox.SelectedValuePath = "Value";

            var projectsItems = new ObservableCollection<Object>();
            for (Int32 projectIndex = 0; projectIndex < projects.Objects.Count(); projectIndex++)
            {
                projectsItems.Add(new { Text = projects.Objects.Get(projectIndex).Name, Value = projects.Objects.Get(projectIndex).ID });
            }

            ProjectsComboBox.ItemsSource = projectsItems;
            if (projectToSelect == null)
                ProjectsComboBox.SelectedIndex = 0;
            else
                //ProjectsComboBox.SelectedValue = projectToSelect.GetID(); now not normal behaviour - cretaed object returns without parameter 
                ProjectsComboBox.SelectedValue = projectToSelect.ID; 
        }

        private void DeleteCurrentProject()
        {
            if (MessageBox.Show(String.Format("Are you sure you want to delete {0} project?", _currentProject.GetName()), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                MessageBoxResult.Yes)
            {
                _remoteConnection.DeleteProject(_currentProject.GetID());
                FillProjectsComboBox();
            }
        }

        private void CurrentProjectChanged()
        {
            _currentProject = null;

            var currentProjectId = (String)ProjectsComboBox.SelectedValue;
            if (!String.IsNullOrEmpty(currentProjectId))
            {
                _remoteConnection.OpenProject(currentProjectId);
                _currentProject = _remoteConnection.GetObject(currentProjectId);
            }
            else
            {
                _currentProject = null;
            }
            SetPropertiesPanelEnability(false);
            _currentObject = null;
            _currentParameter = null;
            if (_currentProject != null)
            {
                DeleteCurrentProjectButton.IsEnabled = true;
                SetObjectBrowserPanelEnability(true);
                RefreshObjectBrowserTreeView();
            }
            else
            {
                SetObjectBrowserPanelEnability(false);
                DeleteCurrentProjectButton.IsEnabled = false;
            }
        }

        private void SetProjectPanelEnability(Boolean status)
        {
            ProjectPanel.IsEnabled = status;
            if (!status)
            {
                _currentProject = null;
                ProjectsComboBox.ItemsSource = null;
                ProjectsComboBox.Items.Clear();
            }
        }

        private void SetProjectPanelToCreatingNewProjectView()
        {
            ProjectCreatingPanel.Visibility = Visibility.Visible;
            ProjectSelectingPanel.Visibility = Visibility.Collapsed;
        }

        private void SetProjectPanelToSelectingProjectView()
        {
            ProjectCreatingPanel.Visibility = Visibility.Collapsed;
            ProjectSelectingPanel.Visibility = Visibility.Visible;
        }

        private void CreateNewProject()
        {
            var newProjectName = ProjectNameTextBox.Text;
            if (String.IsNullOrEmpty(newProjectName))
            {
                MessageBox.Show("Project name can not be empty. Please enter the name.");
                return;
            }
            
            RemoteObj projects = _remoteConnection.GetProjects();
            for (int i = 0; i < projects.Objects.Count(); i++)
            {
                if (projects.Objects.Get(i).Name == newProjectName)
                {
                    MessageBox.Show("Sorry you already have a project with this name. Try again.");
                    return;
                }
            }

            RemoteObj createdObject = _remoteConnection.CreateProject(newProjectName);
            SetProjectPanelToSelectingProjectView();
            FillProjectsComboBox(createdObject);
            CurrentProjectChanged();
            UserPanel.IsEnabled = true;
        }

        #endregion



        #region Object Browser Panel Methods

        private void SelectedNodeChanged(TreeViewEventArgs e)
        {
            if (_currentProject == null) return;
            if (e.Node.Nodes.Count == 0)
            {
                String objectId = (String)e.Node.Parent.Tag;
                String parameterName = (String)e.Node.Tag;
                RemoteObj tmpObject = _currentProject.GetObject(objectId);
                RemoteParam parameter = tmpObject.GetParam(parameterName);
                _currentParameter = parameter;
                _currentObject = null;
                SetObjectPropertiesPanelEnability(false);
                SetParameterPropertiesPanelEnability(true);
                FillParameterFieldsPanelByParameter(parameter);
                if (!ParameterFieldsExpander.IsExpanded)
                    ParameterFieldsExpander.IsExpanded = true;
            }
            else
            {
                String objectId = (String)e.Node.Tag;
                DeleteCurrentObjectButton.IsEnabled = objectId != _currentProject.GetID();
                _currentObject = _currentProject.GetObject(objectId);
                _currentParameter = null;
                CreateNewObjectButton.IsEnabled = DrawGeometriesOfObjectButton.IsEnabled = CheckIfObjectCanContainAnotherObject(_currentObject);
                SetObjectPropertiesPanelEnability(true);
                SetParameterPropertiesPanelEnability(false);
            }
            SetParameterPropertiesPanelToEditingView();
        }

        private Boolean CheckIfObjectCanContainAnotherObject(RemoteObj currentObject)
        {
            ObjectType objectType;
            if (!Enum.TryParse(currentObject.GetType(), true, out objectType))
                return false;
            if (ObjectsHelper.GetAllowedTypes(objectType) == null)
                return false;
            return true;
        }

        internal void RefreshObjectBrowserTreeView()
        {
            _currentObject = null;
            _currentParameter = null;
            _winFormsTreeView.Nodes.Clear();
            TreeNode newNode = new TreeNode();
            _winFormsTreeView.Nodes.Add(newNode);
            PopulateObjectsBrowserWinFormsTree(_currentProject, newNode);
            if (_currentObject == null)
                _winFormsTreeView.SelectedNode = newNode;
            else
                _winFormsTreeView.SelectedNode = GetNodeByObjectId(_currentObject.GetID());
        }

        private TreeNode GetNodeByObjectId(String objectId)
        {
            TreeNode currentNode = null;
            foreach (TreeNode node in _winFormsTreeView.Nodes)
            {
                currentNode = CheckChildrenNodesForId(objectId, node);
                if (currentNode != null) break;
            }
            return currentNode;
        }

        public TreeNode CheckChildrenNodesForId(String itemId, TreeNode rootNode)
        {
            foreach (TreeNode node in rootNode.Nodes)
            {
                if (node.Tag.ToString() == itemId) return node;
                TreeNode next = CheckChildrenNodesForId(itemId, node);
                if (next != null) return next;
            }
            return null;
        }

        private void PopulateObjectsBrowserWinFormsTree(RemoteObj remoteObject, TreeNode node)
        {
            String id = "";
            String name = "";
            String type = "";

            for (Int32 parameterIndex = 0; parameterIndex < remoteObject.Params.Count(); parameterIndex++)
            {
                RemoteParam currentParameter = remoteObject.Params.Get(parameterIndex);
                if (currentParameter.Name == "ID") id = currentParameter.Value.AsText();
                if (currentParameter.Name == "N") name = currentParameter.Value.AsText();
                if (currentParameter.Name == "T") type = currentParameter.Value.AsText();

                TreeNode childNode = new TreeNode();
                childNode.Name = currentParameter.Parent.GetID() + "_" + currentParameter.Name;
                node.Nodes.Add(childNode);
                childNode.Text = currentParameter.Name + " = " + currentParameter.Value.AsText();
                childNode.Tag = currentParameter.Name;
            }

            node.Text = name + "(" + type + ")";
            node.Tag = id;

            for (Int32 objectIndex = 0; objectIndex < remoteObject.Objects.Count(); objectIndex++)
            {
                TreeNode st = new TreeNode();
                node.Nodes.Add(st);
                node.Name = remoteObject.Objects.Get(objectIndex).GetID();
                PopulateObjectsBrowserWinFormsTree(remoteObject.Objects.Get(objectIndex), st);
            }
        }

        private void ClearObjectBrowser()
        {
            _winFormsTreeView.Nodes.Clear();
        }

        private void SetObjectBrowserPanelEnability(Boolean status)
        {
            ObjectBrowserPanel.IsEnabled = status;
            ClearObjectBrowser();
        }

        #endregion



        #region Object Properties Panel Methods

        private void SetObjectPropertiesPanelEnability(Boolean status)
        {
            ObjectPropertiesPanel.IsEnabled = status;
        }

        private void AddNewObject()
        {
            SetControlToCreatingNewObjectView();
        }

        private void DeleteCurrentObject()
        {
            DeleteObject(_currentObject.GetID());
        }

        internal Boolean DeleteObject(String currentObjectId)
        {
            if (MessageBox.Show(String.Format("Are you sure you want to delete {0} object?", _currentObject.GetName()), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                MessageBoxResult.Yes)
            {
                _remoteConnection.DeleteObject(currentObjectId);
                TreeNode currentObjectNode = GetNodeByObjectId(currentObjectId);
                TreeNode parentNode = currentObjectNode.Parent;
                var parentObject = _currentProject.GetObject(parentNode.Tag.ToString());
                var currentObject = _currentProject.GetObject(currentObjectId);
                parentObject.Objects.Remove(currentObject);
                RefreshObjectBrowserTreeView();
                return true;
            }
            return false;
        }

        private void SetObjectPropertiesPanelToCreatingNewObjectView()
        {
            ObjectEditingPanel.Visibility = Visibility.Collapsed;
            ObjectCreatingPanel.Visibility = Visibility.Visible;
            FillObjectCreatingPanel();
        }

        private void SetObjectPropertiesPanelToEditingView()
        {
            ObjectEditingPanel.Visibility = Visibility.Visible;
            ObjectCreatingPanel.Visibility = Visibility.Collapsed;
        }

        private void FillObjectCreatingPanel()
        {
            ObjectType parentType = (ObjectType) Enum.Parse(typeof (ObjectType), _currentObject.GetType());
            NewObjectParentTypeComboBox.SelectedValue = parentType;

            ClearComboBox(NewObjectTypeComboBox);
            var typesItems = new ObservableCollection<Object>();
            foreach (var currentType in ObjectsHelper.GetAllowedTypes(parentType))
            {
                typesItems.Add(currentType);
            }
            NewObjectTypeComboBox.ItemsSource = typesItems;
        }

        private void CreateNewObjectByInputedValues()
        {
            var parentObjectId = _currentObject.GetID();
            var newObjectName = NewObjectNameTextBox.Text;
            if (NewObjectTypeComboBox.SelectedValue == null)
            {
                MessageBox.Show("Object type can not be undefined. Please select the type.");
                return;                
            }
            var newObjectType = (ObjectType)NewObjectTypeComboBox.SelectedValue;
            ExecuteCreatingNewObjectCommand(parentObjectId, newObjectName, newObjectType.ToString());
        }

        internal void ExecuteCreatingNewObjectCommand(String parentObjectId, String newObjectName, String newObjectType)
        {
            var createdObject = _remoteConnection.CreateObject(parentObjectId, newObjectType, newObjectName);
            SetControlToDefaultViewAfterCreatingNewObjectView();
        }

        private void CancelCreatingObject()
        {
            SetControlToDefaultViewAfterCreatingNewObjectView();
        }

        private void ClearObjectCreatingPanelFields()
        {
            NewObjectNameTextBox.Text = null;
            ClearComboBox(NewObjectTypeComboBox);
        }


        #endregion



        #region Parameter Properties Panel Methods

        private void FillParameterFieldsPanelByParameter(RemoteParam parameter)
        {
            ParameterNameTextBox.Text = parameter.Name;
            ParameterDescriptionTextBox.Text = parameter.Desc;

            ParameterExpressionTextBox.Text = parameter.Expr;
            ParameterValueTextBox.Text = parameter.Value.AsText();

            if (parameter.Type.ToLower() == "number")
                ParameterTypeComboBox.SelectedIndex = (Byte)ParameterType.Number;
            else
                ParameterTypeComboBox.SelectedIndex = (Byte)ParameterType.Text;

            ParameterUnitTypeComboBox.SelectedIndex = RemoteParam.StringToUnitType(parameter.UType);

            ParameterUnitCategoryTextBox.Text = parameter.UCat;

            ParameterDisplayRoleComboBox.SelectedIndex = (parameter.DisplayRole == "userinput" ? 1 : 0);

            ParameterDisplayCategoryTextBox.Text = parameter.DisplayCategory;

            if (parameter.DisplayWidth == null)
                ParameterDisplayWidthComboBox.SelectedIndex = (Byte)ParameterDisplayWidth.One;
            else
                ParameterDisplayWidthComboBox.SelectedIndex = Int32.Parse(parameter.DisplayWidth);
        }

        private void SetParameterPropertiesPanelToEditingView()
        {
            DeleteCurrentParameterButton.Visibility = Visibility.Visible;
            SaveParameterButton.Content = "Apply";
            CancelParameterButton.Content = "Revert";
        }

        private void SetParameterPropertiesPanelToCreatingNewParameterView()
        {
            DeleteCurrentParameterButton.Visibility = Visibility.Collapsed;
            SaveParameterButton.Content = "Create";
            CancelParameterButton.Content = "Cancel";
            if (!ParameterFieldsExpander.IsExpanded)
                ParameterFieldsExpander.IsExpanded = true;
        }

        private void SetParameterPropertiesPanelEnability(Boolean status)
        {
            ParameterPropertiesPanel.IsEnabled = status;
            if (!status)
                ClearParameterPropertiesPanel();
        }

        private void ClearParameterPropertiesPanel()
        {
            ParameterNameTextBox.Text = null;
            ParameterTypeComboBox.SelectedValue = null;
            ParameterExpressionTextBox.Text = null;
            ParameterValueTextBox.Text = null;
            ParameterDescriptionTextBox.Text = null;
            ParameterUnitTypeComboBox.SelectedValue = null;
            ParameterUnitCategoryTextBox.Text = null;
            ParameterDisplayRoleComboBox.SelectedValue = null;
            ParameterDisplayCategoryTextBox.Text = null;
            ParameterDisplayWidthComboBox.SelectedValue = null;
        }

        private void DeleteCurrentParameter()
        {
            if (MessageBox.Show(String.Format("Are you sure you want to delete {0} parameter?", _currentParameter.Name), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                 MessageBoxResult.Yes)
            {
                var parentObject = _currentParameter.Parent;
                _remoteConnection.DeleteParam(parentObject.GetID(), _currentParameter.Name);
                parentObject.Params.Remove(_currentParameter);
                RefreshObjectBrowserTreeView();
            }
        }

        private void ApplyChangeToParameter()
        {
            UpdateParameterFieldsByInputData(_currentParameter);
            UpdateParameter(_currentParameter);
        }

        private void UpdateParameter(RemoteParam parameter)
        {
            parameter.Update();
        }

        private void UpdateParameterFieldsByInputData(RemoteParam parameter)
        {
            parameter.Name = ParameterNameTextBox.Text;
            parameter.Desc = ParameterDescriptionTextBox.Text;

            parameter.Expr = ParameterExpressionTextBox.Text;

            if (ParameterTypeComboBox.SelectedValue != null)
                parameter.Type = ((ParameterType) ParameterTypeComboBox.SelectedValue).ToString();
            else
                parameter.Type = null;

            parameter.UType = RemoteParam.UnitTypeToString(ParameterUnitTypeComboBox.SelectedIndex);
           
            parameter.UCat = ParameterUnitCategoryTextBox.Text;

            if (ParameterDisplayRoleComboBox.SelectedValue != null)
                parameter.DisplayRole = ((ParameterDisplayRole)ParameterDisplayRoleComboBox.SelectedValue).ToString();
            else
                parameter.DisplayRole = null;

            parameter.DisplayCategory = ParameterDisplayCategoryTextBox.Text;

            if (ParameterDisplayWidthComboBox.SelectedValue == null || (ParameterDisplayWidth)ParameterDisplayWidthComboBox.SelectedValue == ParameterDisplayWidth.One)
                parameter.DisplayWidth = null;
            else
                parameter.DisplayWidth = ParameterDisplayWidthComboBox.SelectedIndex.ToString();
        }

        private void RevertParameterState()
        {
            FillParameterFieldsPanelByParameter(_currentParameter);
        }

        private void CreateNewParameterByInputedValues()
        {
            RemoteParam newParameter = new RemoteParam(_remoteConnection, _currentObject);
            UpdateParameterFieldsByInputData(newParameter);
            var createdParameter = _remoteConnection.CreateParam(_currentObject.GetID(), newParameter.Name, newParameter.Type, newParameter.Expr, newParameter.Desc, newParameter.UType, newParameter.UCat);
            SetControlToDefaultViewAfterCreatingNewParameterView();
            CurrentProjectChanged();
        }

        private void CancelCreatingParameter()
        {
            SetControlToDefaultViewAfterCreatingNewParameterView();
        }


        #endregion



        #region Common Control Helpers Methods

        private void ClearComboBox(System.Windows.Controls.ComboBox comboBox)
        {
            comboBox.ItemsSource = null;
            comboBox.SelectedValue = null;
            comboBox.Items.Clear();
        }
    
        private void ShowLoadingProcess()
        {
            WinFormsTreeViewHolder.Visibility = Visibility.Collapsed;
            LoadingViewBox.Visibility = Visibility.Visible;
        }

        private void HideLoadingProcess()
        {
            LoadingViewBox.Visibility = Visibility.Collapsed;
            WinFormsTreeViewHolder.Visibility = Visibility.Visible;
        }

        private void SetControlToCreatingNewProjectView()
        {
            UserPanel.IsEnabled = false;
            ObjectBrowserPanel.IsEnabled = false;
            UpdateDataButton.IsEnabled = false;
            ObjectPropertiesPanel.IsEnabled = false;
            ParameterPropertiesPanel.IsEnabled = false;
            SetProjectPanelToCreatingNewProjectView();
        }

        private void SetControlToCreatingNewParameterView()
        {
            UserPanel.IsEnabled = false;
            ProjectPanel.IsEnabled = false;
            ObjectBrowserPanel.IsEnabled = false;
            UpdateDataButton.IsEnabled = false;
            ObjectPropertiesPanel.IsEnabled = false;
            SetParameterPropertiesPanelToCreatingNewParameterView();
            SetParameterPropertiesPanelEnability(true);
        }

        private void SetControlToDefaultViewAfterCreatingNewParameterView()
        {
            UserPanel.IsEnabled = true;
            ProjectPanel.IsEnabled = true;
            ObjectBrowserPanel.IsEnabled = true;
            UpdateDataButton.IsEnabled = true;
            ObjectPropertiesPanel.IsEnabled = true;
            SetParameterPropertiesPanelToEditingView();
            SetParameterPropertiesPanelEnability(false);
        }

        private void SetControlToCreatingNewObjectView()
        {
            UserPanel.IsEnabled = false;
            ProjectPanel.IsEnabled = false;
            ObjectBrowserPanel.IsEnabled = false;
            UpdateDataButton.IsEnabled = false;
            ParameterPropertiesPanel.IsEnabled = false;
            SetObjectPropertiesPanelToCreatingNewObjectView();
        }

        private void SetControlToDefaultViewAfterCreatingNewObjectView()
        {
            UserPanel.IsEnabled = true;
            ProjectPanel.IsEnabled = true;
            ObjectBrowserPanel.IsEnabled = true;
            UpdateDataButton.IsEnabled = true;
            ParameterPropertiesPanel.IsEnabled = true;
            SetObjectPropertiesPanelToEditingView();
            ClearObjectCreatingPanelFields();
        }

        internal void RefreshControl()
        {
            ShowLoadingProcess();
            if (_remoteConnection == null)
            {
                UserPanel.IsEnabled = false;
                return;
            }
            UserPanel.IsEnabled = true;
            CheckIfUserIsAuthorized();
            FillProjectsComboBox();
            HideLoadingProcess();
        }

        private void ShowNotImplementedYetMessage()
        {
            MessageBox.Show("Not Implemented Yet!");
        }

        private void CheckIfUserIsAuthorized()
        {
            ShowLoadingProcess();
            if (_remoteConnection.UserIsAuth())
            {
                _currentUserName = String.Format("{1} {0}", _remoteConnection.UserFirstName(), _remoteConnection.UserLastName());
                LogInButton.Visibility = Visibility.Collapsed;
                LogOutButton.Visibility = Visibility.Visible;
            }
            else
            {
                _currentUserName = null;
                LogOutButton.Visibility = Visibility.Collapsed;
                LogInButton.Visibility = Visibility.Visible;
                SetObjectBrowserPanelEnability(false);
                SetPropertiesPanelEnability(false);
            }
            UserFullName.Text = _currentUserName;
            _currentProject = null;
            _currentObject = null;
            _currentParameter = null;
            _isUserAuthorized = _currentUserName != null;
            SetProjectPanelEnability(_isUserAuthorized);
            UpdateDataButton.IsEnabled = _isUserAuthorized;
            HideLoadingProcess();
        }

        private void SetPropertiesPanelEnability(Boolean status)
        {
            SetObjectPropertiesPanelEnability(status);
            SetParameterPropertiesPanelEnability(status);
        }

        private void DrawGeometriesOfObject()
        {
            _parent.DrawGeometriesOfObject(_currentObject);
        }

        #endregion



        #region Event Handlers

        void _winFormsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectedNodeChanged(e);
        }

        private void ProjectsComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentProjectChanged();
        }

        private void UpdateDataButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_remoteConnection == null) return;
            if (!_isUserAuthorized)
                LogIn();
            RefreshControl();
        }

        private void CreateNewProjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetControlToCreatingNewProjectView();
        }
        
        private void DeleteCurrentProjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            DeleteCurrentProject();
        }

        private void LogInButton_OnClick(object sender, RoutedEventArgs e)
        {
            LogIn();
        }

        private void LogOutButton_OnClick(object sender, RoutedEventArgs e)
        {
            LogOut();
        }
     
        private void CreateNewObjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            AddNewObject();
        }

        private void DeleteCurrentObjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            DeleteCurrentObject();
        }

        private void CreateNewParameterButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetControlToCreatingNewParameterView();
            if (!ParameterFieldsExpander.IsExpanded)
                ParameterFieldsExpander.IsExpanded = true;
        }

        private void DeleteCurrentParameterButton_OnClick(object sender, RoutedEventArgs e)
        {
            DeleteCurrentParameter();
        }

        private void SaveParameterButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentParameter == null)
                CreateNewParameterByInputedValues();
            else
                ApplyChangeToParameter();
        }

        private void CancelParameterButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentParameter == null)
                CancelCreatingParameter();
            else
                RevertParameterState();
        }

        private void DrawGeometriesOfObjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            DrawGeometriesOfObject();
        }
        
        private void OkCreatingProjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            CreateNewProject();
            ProjectNameTextBox.Text = null;
        }

        private void CancelCreatingProjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetProjectPanelToSelectingProjectView();
            CurrentProjectChanged();
            UserPanel.IsEnabled = true;
            ProjectNameTextBox.Text = null;
        }

        private void OkCreatingObjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            CreateNewObjectByInputedValues();
        }

        private void CancelCreatingObjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            CancelCreatingObject();
        }


        #endregion
    }
}