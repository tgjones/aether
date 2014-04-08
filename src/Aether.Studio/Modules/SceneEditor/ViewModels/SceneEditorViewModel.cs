using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Aether.IO;
using Aether.Renderers;
using Gemini.Framework.Services;
using Gemini.Modules.CodeEditor;
using Gemini.Modules.CodeEditor.ViewModels;
using Gemini.Modules.CodeEditor.Views;
using Gemini.Modules.ErrorList;

namespace Aether.Studio.Modules.SceneEditor.ViewModels
{
    [Export(typeof(SceneEditorViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SceneEditorViewModel : CodeEditorViewModel
    {
        private readonly IErrorList _errorList;
        private readonly IShell _shell;
        private WriteableBitmap _outputBitmap;
        private ICodeEditorView _codeEditorView;
        private CancellationTokenSource _cancellationTokenSource;

        public WriteableBitmap OutputBitmap
        {
            get { return _outputBitmap; }
            set
            {
                _outputBitmap = value;
                NotifyOfPropertyChange(() => OutputBitmap);
            }
        }

        [ImportingConstructor]
        public SceneEditorViewModel(LanguageDefinitionManager languageDefinitionManager,
            IErrorList errorList, IShell shell)
            : base(languageDefinitionManager)
        {
            _errorList = errorList;
            _shell = shell;
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            _codeEditorView = (ICodeEditorView) view;
            _codeEditorView.TextEditor.TextChanged += OnTextChanged;

            StartRendering(_codeEditorView.TextEditor.Text);
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            _errorList.Items.Clear();

            // Cancel any existing render task.
            if (_cancellationTokenSource != null)
                _cancellationTokenSource.Cancel();

            StartRendering(_codeEditorView.TextEditor.Text);
        }

        private void StartRendering(string pbrtCode)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            Scene scene;
            Renderer renderer;
            try
            {
                SceneReader.Read(new StringReader(pbrtCode), out scene, out renderer);
            }
            catch (Exception ex)
            {
                _errorList.AddItem(ErrorListItemType.Error, ex.Message, Path);
                _shell.ShowTool(_errorList);
                return;
            }

            OutputBitmap = renderer.Output;

            Task.Factory.StartNew(() => renderer.Render(scene, token), token)
                .ContinueWith(t =>
                {
                    switch (t.Status)
                    {
                        case TaskStatus.Faulted:
                            _errorList.AddItem(ErrorListItemType.Error, t.Exception.Message);
                            break;
                    }
                });
        }
    }
}