#pragma once


#include "Network.h"
#include "Constants2.h"
#include "Utils.h"

using namespace Threading;


/*void ThreadProc(Object^ data)
{
//NetworkClient^ nc = gcnew NetworkClient(lH);

//TimerCallback^ tcb = gcnew TimerCallback(nc, &NetworkClient::UpdateEvents);

//Threading::Timer^ stateTimer = gcnew Threading::Timer(tcb, nullptr, 30, 30);

//while (true)
//{
//	Thread::Sleep(30);
//}

}*/


namespace TnetServerPrototype {

	using namespace System;
	using namespace System::ComponentModel;
	using namespace System::Collections;
	using namespace System::Windows::Forms;
	using namespace System::Data;
	using namespace System::Drawing;

	/// <summary>
	/// Summary for MainForm
	/// </summary>
	public ref class MainForm2 : public System::Windows::Forms::Form
	{
	public:
		MainForm2(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~MainForm2()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::StatusStrip^  statusStrip1;
	protected:
	private: System::Windows::Forms::ToolStripStatusLabel^  tatusLabel;
	private: System::Windows::Forms::MenuStrip^  menuStrip1;
	private: System::Windows::Forms::ToolStripMenuItem^  fILEToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  hELPToolStripMenuItem;
	private: System::Windows::Forms::TabControl^  tabControl1;
	private: System::Windows::Forms::TabPage^  tabPage1;
	private: System::Windows::Forms::TabPage^  tabPage2;
	private: System::Windows::Forms::RichTextBox^  richTextBox_Log;
	private: System::Windows::Forms::Timer^  timer1;
	private: System::ComponentModel::IContainer^  components;

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>


#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->components = (gcnew System::ComponentModel::Container());
			this->statusStrip1 = (gcnew System::Windows::Forms::StatusStrip());
			this->tatusLabel = (gcnew System::Windows::Forms::ToolStripStatusLabel());
			this->menuStrip1 = (gcnew System::Windows::Forms::MenuStrip());
			this->fILEToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->hELPToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->tabControl1 = (gcnew System::Windows::Forms::TabControl());
			this->tabPage1 = (gcnew System::Windows::Forms::TabPage());
			this->richTextBox_Log = (gcnew System::Windows::Forms::RichTextBox());
			this->tabPage2 = (gcnew System::Windows::Forms::TabPage());
			this->timer1 = (gcnew System::Windows::Forms::Timer(this->components));
			this->statusStrip1->SuspendLayout();
			this->menuStrip1->SuspendLayout();
			this->tabControl1->SuspendLayout();
			this->tabPage1->SuspendLayout();
			this->SuspendLayout();
			// 
			// statusStrip1
			// 
			this->statusStrip1->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(1) { this->tatusLabel });
			this->statusStrip1->Location = System::Drawing::Point(0, 361);
			this->statusStrip1->Name = L"statusStrip1";
			this->statusStrip1->Size = System::Drawing::Size(718, 22);
			this->statusStrip1->TabIndex = 0;
			this->statusStrip1->Text = L"statusStrip1";
			// 
			// tatusLabel
			// 
			this->tatusLabel->Name = L"tatusLabel";
			this->tatusLabel->Size = System::Drawing::Size(39, 17);
			this->tatusLabel->Text = L"Ready";
			// 
			// menuStrip1
			// 
			this->menuStrip1->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(2) {
				this->fILEToolStripMenuItem,
					this->hELPToolStripMenuItem
			});
			this->menuStrip1->Location = System::Drawing::Point(0, 0);
			this->menuStrip1->Name = L"menuStrip1";
			this->menuStrip1->Size = System::Drawing::Size(718, 24);
			this->menuStrip1->TabIndex = 1;
			this->menuStrip1->Text = L"menuStrip1";
			// 
			// fILEToolStripMenuItem
			// 
			this->fILEToolStripMenuItem->Name = L"fILEToolStripMenuItem";
			this->fILEToolStripMenuItem->Size = System::Drawing::Size(40, 20);
			this->fILEToolStripMenuItem->Text = L"FILE";
			// 
			// hELPToolStripMenuItem
			// 
			this->hELPToolStripMenuItem->Name = L"hELPToolStripMenuItem";
			this->hELPToolStripMenuItem->Size = System::Drawing::Size(47, 20);
			this->hELPToolStripMenuItem->Text = L"HELP";
			// 
			// tabControl1
			// 
			this->tabControl1->Controls->Add(this->tabPage1);
			this->tabControl1->Controls->Add(this->tabPage2);
			this->tabControl1->Dock = System::Windows::Forms::DockStyle::Fill;
			this->tabControl1->Location = System::Drawing::Point(0, 24);
			this->tabControl1->Name = L"tabControl1";
			this->tabControl1->SelectedIndex = 0;
			this->tabControl1->Size = System::Drawing::Size(718, 337);
			this->tabControl1->TabIndex = 2;
			// 
			// tabPage1
			// 
			this->tabPage1->Controls->Add(this->richTextBox_Log);
			this->tabPage1->Location = System::Drawing::Point(4, 22);
			this->tabPage1->Name = L"tabPage1";
			this->tabPage1->Padding = System::Windows::Forms::Padding(3);
			this->tabPage1->Size = System::Drawing::Size(710, 311);
			this->tabPage1->TabIndex = 0;
			this->tabPage1->Text = L"Log";
			this->tabPage1->UseVisualStyleBackColor = true;
			// 
			// richTextBox_Log
			// 
			this->richTextBox_Log->Dock = System::Windows::Forms::DockStyle::Fill;
			this->richTextBox_Log->Location = System::Drawing::Point(3, 3);
			this->richTextBox_Log->Name = L"richTextBox_Log";
			this->richTextBox_Log->Size = System::Drawing::Size(704, 305);
			this->richTextBox_Log->TabIndex = 0;
			this->richTextBox_Log->Text = L"";
			// 
			// tabPage2
			// 
			this->tabPage2->Location = System::Drawing::Point(4, 22);
			this->tabPage2->Name = L"tabPage2";
			this->tabPage2->Padding = System::Windows::Forms::Padding(3);
			this->tabPage2->Size = System::Drawing::Size(710, 311);
			this->tabPage2->TabIndex = 1;
			this->tabPage2->Text = L"Settings";
			this->tabPage2->UseVisualStyleBackColor = true;
			// 
			// timer1
			// 
			this->timer1->Enabled = true;
			this->timer1->Interval = 20;
			this->timer1->Tick += gcnew System::EventHandler(this, &MainForm2::timer1_Tick);
			// 
			// MainForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(718, 383);
			this->Controls->Add(this->tabControl1);
			this->Controls->Add(this->statusStrip1);
			this->Controls->Add(this->menuStrip1);
			this->MainMenuStrip = this->menuStrip1;
			this->Name = L"MainForm";
			this->Text = L"TNET SERVER v0.1";
			this->Load += gcnew System::EventHandler(this, &MainForm2::MainForm_Load);
			this->statusStrip1->ResumeLayout(false);
			this->statusStrip1->PerformLayout();
			this->menuStrip1->ResumeLayout(false);
			this->menuStrip1->PerformLayout();
			this->tabControl1->ResumeLayout(false);
			this->tabPage1->ResumeLayout(false);
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion


		NetworkClient^ nc = gcnew NetworkClient();

		void PrintMessage(String^ msg)
		{
			richTextBox_Log->AppendText("\n" + msg);
		}

		void PrintMessage(string msg)
		{
			richTextBox_Log->AppendText("\n" + StringUtils::stops(msg));
		}


	private: System::Void MainForm_Load(System::Object^  sender, System::EventArgs^  e) {

		global_db.open(".\\db\\ledger.dat");
	}


	private: System::Void timer1_Tick(System::Object^  sender, System::EventArgs^  e) {

		nc->UpdateEvents("");

		//richTextBox_Log->AppendText(".");
	}


	};
}
