namespace VocabularyTrainer;

partial class QuizForm
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.Label questionLabel;
    private System.Windows.Forms.Button answerButton1;
    private System.Windows.Forms.Button answerButton2;
    private System.Windows.Forms.Button answerButton3;
    private System.Windows.Forms.Label resultLabel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        questionLabel = new System.Windows.Forms.Label();
        answerButton1 = new System.Windows.Forms.Button();
        answerButton2 = new System.Windows.Forms.Button();
        answerButton3 = new System.Windows.Forms.Button();
        resultLabel = new System.Windows.Forms.Label();
        SuspendLayout();

        // questionLabel
        questionLabel.AutoSize = true;
        questionLabel.MaximumSize = new System.Drawing.Size(450, 0);
        questionLabel.Location = new System.Drawing.Point(20, 20);
        questionLabel.Text = "Question";

        // answerButton1
        answerButton1.AutoSize = true;
        answerButton1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        answerButton1.Click += AnswerClicked;

        // answerButton2
        answerButton2.AutoSize = true;
        answerButton2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        answerButton2.Click += AnswerClicked;

        // answerButton3
        answerButton3.AutoSize = true;
        answerButton3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        answerButton3.Click += AnswerClicked;

        // resultLabel
        resultLabel.AutoSize = true;
        resultLabel.Location = new System.Drawing.Point(20, 155);

        // QuizForm
        // ClientSize = new System.Drawing.Size(380, 300);
        Controls.Add(questionLabel);
        Controls.Add(answerButton1);
        Controls.Add(answerButton2);
        Controls.Add(answerButton3);
        Controls.Add(resultLabel);
        Text = "Vocabulary Trainer";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

        ResumeLayout(false);
        PerformLayout();
    }
}