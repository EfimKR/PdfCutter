namespace PdfCutter
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.Win32;
    using PdfSharp.Drawing;
    using PdfSharp.Pdf;
    using PdfSharp.Pdf.IO;
    using Prism.Commands;
    using Prism.Mvvm;

    public class SplitPageInHalfViewModel : BindableBase
    {
        private bool isBusy;
        private string fileName;
        private bool fileHandled;
        private PdfDocument outputFile;
        private bool openFileAfterSave;

        public SplitPageInHalfViewModel()
        {
            this.OpenFileCommand = new DelegateCommand(this.ExecuteOpenFileCommand, this.CanExecuteOpenFileCommand);
            this.SaveFileCommand = new DelegateCommand(this.ExecuteSaveFileCommand, this.CanExecuteSaveFileCommand);
            this.HandleFileCommand = new DelegateCommand(this.ExecuteHandleFileCommand, this.CanExecuteHandleFileCommand);
        }

        #region Property

        public bool IsBusy
        {
            get { return this.isBusy; }

            set
            {
                if (!this.SetProperty(ref this.isBusy, value)) return;

                this.UpdateCanExecute();
            }
        }

        public string FileName
        {
            get { return this.fileName; }

            set { this.SetProperty(ref this.fileName, value); }
        }

        public bool OpenFileAfterSave
        {
            get { return this.openFileAfterSave; }

            set { this.SetProperty(ref this.openFileAfterSave, value); }
        }

        public DelegateCommand OpenFileCommand { get; private set; }

        public DelegateCommand SaveFileCommand { get; private set; }

        public DelegateCommand HandleFileCommand { get; private set; }

        #endregion

        private void UpdateCanExecute()
        {
            this.OpenFileCommand.RaiseCanExecuteChanged();
            this.SaveFileCommand.RaiseCanExecuteChanged();
            this.HandleFileCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteSaveFileCommand()
        {
            return !this.isBusy && this.fileHandled;
        }

        private bool CanExecuteOpenFileCommand()
        {
            return !this.isBusy;
        }

        private bool CanExecuteHandleFileCommand()
        {
            return !(this.isBusy || this.fileHandled || object.ReferenceEquals(this.fileName, null));
        }

        private void ExecuteSaveFileCommand()
        {
            var dlg = new SaveFileDialog { DefaultExt = ".pdf", Filter = "Pdf file (*.pdf)|*.pdf" };
            if (dlg.ShowDialog() == true)
            {
                this.outputFile.Save(dlg.FileName);
                if (this.openFileAfterSave)
                {
                    Process.Start(dlg.FileName);
                }
            }
        }

        private void ExecuteOpenFileCommand()
        {
            var d = new OpenFileDialog { DefaultExt = ".pdf", Filter = "Pdf file (*.pdf)|*.pdf", Multiselect = false };

            if (d.ShowDialog() == true)
            {
                this.FileName = d.FileName;
                this.fileHandled = false;
                this.UpdateCanExecute();
            }
        }

        private void ExecuteHandleFileCommand()
        {
            this.isBusy = true;
            this.UpdateCanExecute();

            Task.Factory.StartNew(this.SplitPdf).ContinueWith(t =>
                {
                    this.fileHandled = true;
                    this.IsBusy = false;
                });
        }

        private void SplitPdf()
        {
            var output = new PdfDocument();
            var input = PdfReader.Open(this.fileName, PdfDocumentOpenMode.Import);
            for (int i = 0; i < input.PageCount; i++)
            {
                var page = input.Pages[i];

                // if heigt bigger width cut horizontally
                // weird orientation pages in dock
                if (page.Height > page.Width)
                {
                    var halfHeight = page.Height / 2;
                    var sz = new XSize(page.Width, halfHeight);

                    page.CropBox = new PdfRectangle(new XPoint(0, 0), sz);
                    output.AddPage(page);

                    page.CropBox = new PdfRectangle(new XPoint(0, halfHeight), sz);
                    output.AddPage(page);
                }
                else
                {
                    var halfWidth = page.Width / 2;
                    var sz = new XSize(halfWidth, page.Height);

                    page.CropBox = new PdfRectangle(new XPoint(0, 0), sz);
                    output.AddPage(page);

                    page.CropBox = new PdfRectangle(new XPoint(halfWidth, 0), sz);
                    output.AddPage(page);
                }
            }

            this.outputFile = output;
        }
    }
}