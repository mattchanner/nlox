using System.Reflection;
using System.Text;

using Alsing.Windows.Forms;

using Lox;
using Lox.Lang;
using WeifenLuo.WinFormsUI.Docking;
using Environment = System.Environment;

namespace LoxEditor
{
    public class MainForm : Form
    {
        private readonly LoxLang lox = new();
        private ToolStripContainer toolStripContainer;
        private MenuStrip menuStrip;
        private SyntaxBoxControl editor;
        private RichTextBox output;
        private TreeView astTree;
        private ToolStripButton runButton;

        public MainForm()
        {
            InitializeComponents();

            AutoScaleMode = AutoScaleMode.Dpi;

            Console.SetOut(new ConsoleWriter(this.output, Color.Gray));
            Console.SetError(new ConsoleWriter(this.output, Color.Red));
        }

        private void InitializeComponents()
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.Size = new Size(1100, 800);
            this.Text = "Lox Editor";
            
            this.toolStripContainer = new();
            this.toolStripContainer.Dock = DockStyle.Fill;
            this.Controls.Add(this.toolStripContainer);

            this.editor = new();
            this.editor.Dock = DockStyle.Fill;
            this.editor.WhitespaceColor = System.Drawing.SystemColors.ControlDark;
            this.editor.FontName = "Consolas";
            this.editor.FontSize = 18;
            this.editor.Document.SetSyntaxFromEmbeddedResource(Assembly.GetExecutingAssembly(), "LoxEditor.Lox.syn");

            this.output = new();
            this.output.Dock = DockStyle.Fill;
            this.output.Font = new Font("Consolas", 12f, FontStyle.Regular);
            
            this.menuStrip = new();
            this.menuStrip.RenderMode = ToolStripRenderMode.System;
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.menuStrip);
            this.runButton = new("Run");
            this.runButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.menuStrip.Items.Add(this.runButton);
            this.runButton.Click += OnRunClicked;

            this.astTree = new();
            this.astTree.Dock = DockStyle.Fill;

            DockPanel dockPanel = new DockPanel();
            dockPanel.Dock = DockStyle.Fill;
            dockPanel.Theme = new VS2015LightTheme();
            this.toolStripContainer.ContentPanel.Controls.Add(dockPanel);

            DockContent treeContent = new();
            treeContent.Controls.Add(this.astTree);
            treeContent.TabText = "Syntax Tree";
            treeContent.Show(dockPanel, DockState.DockRight);

            DockContent center = new();
            center.Controls.Add(editor);
            center.TabText = "Editor";
            center.Show(dockPanel, DockState.Document);
            
            DockContent bottomContent = new();
            bottomContent.TabText = "Output";
            bottomContent.Controls.Add(output);
            bottomContent.Show(dockPanel, DockState.DockBottom);
        }

        private void OnRunClicked(object? sender, EventArgs e)
        {
            this.output.Clear();
            try
            {
                lox.Run(this.editor.Document.Text);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

            DisplayAst(this.editor.Document.Text);
        }

        private void DisplayAst(string source)
        {
            Scanner scanner = new(source, lox);
            List<Token> tokens = scanner.ScanTokens();
            Parser parser = new(tokens, lox);
            List<Stmt> statements = parser.Parse();

            this.astTree.Nodes.Clear();
            if (statements.Count > 0)
            {
                new AstTreeWriter(astTree, lox).Display(statements);
            }
            
        }
    }

    class ConsoleWriter(RichTextBox textbox, Color colour) : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine()
        {
            textbox.Text += Environment.NewLine;
        }

        public override void WriteLine(string? value)
        {
            int currentSelectionEnd = textbox.Text.Length;
            textbox.Text += value + Environment.NewLine;
            textbox.SelectionStart = currentSelectionEnd;
            textbox.SelectionLength = textbox.Text.Length - textbox.SelectionStart;
            textbox.SelectionColor = colour;
        }
    }
}
