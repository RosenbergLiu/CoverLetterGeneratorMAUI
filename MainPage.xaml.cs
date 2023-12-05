using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;

namespace CoverLetterGeneratorMAUI;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }


    private async void CreateDocBtn_ClickedAsync(object sender, EventArgs e)
    {
        await CreateTemplateFile();
    }


    private async Task CreateTemplateFile()
    {
        var folderResult = await FolderPicker.Default.PickAsync();
        if (!folderResult.IsSuccessful) { return; }
        if (folderResult.Folder == null) { return; }

        string filePath = Path.Combine(folderResult.Folder.Path, "CVTemplate.docx");

        try
        {
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                RunProperties italicLightBlue = new RunProperties();
                italicLightBlue.Append(new Italic());
                italicLightBlue.Append(new DocumentFormat.OpenXml.Wordprocessing.Color() { Val = "LightBlue" });
                body.Append(
                    new Paragraph(new Run(italicLightBlue.CloneNode(true), new Text("Hello, this is a sample document created by Cover Letter Generator"))),
                    new Paragraph(new Run(italicLightBlue.CloneNode(true), new Text("Write your cover letter here and use [CompanyName] and [PositionName] to take place company name and position name."))),
                    new Paragraph(new Run(italicLightBlue.CloneNode(true), new Text("Example:"))),
                    new Paragraph(new Run(italicLightBlue.CloneNode(true), new Text(" "))),
                    new Paragraph(new Run(italicLightBlue.CloneNode(true), new Text("Dear Hiring Manager,"))),
                    new Paragraph(new Run(italicLightBlue.CloneNode(true), new Text("I am writing to apply for the [PositionName] position at [CompanyName] ...")))
                );
            }

            DocFileEntry.Text = filePath;
            await DisplayAlert("Success", $"{filePath} \nCreated", "OK");
        }
        catch (System.IO.IOException)
        {
            await DisplayAlert("Template Create Failed", $"{filePath} \nis already exist and in use.", "OK");
        }
    }

    private async void GenerateBtn_Clicked(object sender, EventArgs e)
    {
        if (String.IsNullOrWhiteSpace(CompanyEntry.Text))
        {
            await DisplayAlert("Company name is empty", "Please enter a company name", "OK");
            CompanyEntry.Focus();
            return;
        }

        if (String.IsNullOrWhiteSpace(PositionEntry.Text))
        {
            await DisplayAlert("Postion name is empty", "Please enter a position name", "OK");
            PositionEntry.Focus();
            return;
        }

        if (String.IsNullOrEmpty(DocFileEntry.Text))
        {
            await DisplayAlert("Template file not set", "Please point a template file", "OK");
            return;
        }

        if (!File.Exists(DocFileEntry.Text))
        {
            if (await DisplayAlert("Template not exist", "Would you like to create the template file?", "OK", "Cancel"))
            {
                await CreateTemplateFile();
            }
            return;
        }

        string? docPath = Path.GetDirectoryName(DocFileEntry.Text);
        if (String.IsNullOrEmpty(docPath))
        {
            return;
        }

        string newFilePath = Path.Combine(docPath, "Cover Letter " + CompanyEntry.Text + ".docx");


        File.Copy(DocFileEntry.Text, newFilePath, overwrite: true);

        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(newFilePath, true))
        {
            if (wordDoc.MainDocumentPart != null)
            {
                var body = wordDoc.MainDocumentPart.Document.Body;

                if (body != null)
                {
                    foreach (var text in body.Descendants<Text>())
                    {
                        if (text.Text.Contains("[CompanyName]"))
                        {
                            text.Text = text.Text.Replace("[CompanyName]", CompanyEntry.Text);
                        }

                        if (text.Text.Contains("[PositionName]"))
                        {
                            text.Text = text.Text.Replace("[PositionName]", PositionEntry.Text);
                        }
                    }

                    await DisplayAlert("Success", "Cover letter generated", "OK");
                    CompanyEntry.Text = null;
                    PositionEntry.Text = null;
                }

                wordDoc.MainDocumentPart.Document.Save();
                OutputLabel.Text = newFilePath;
            }
        }
    }

    private async void PickFileBtn_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                DocFileEntry.Text = result.FullPath;
            }
        }catch(Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}