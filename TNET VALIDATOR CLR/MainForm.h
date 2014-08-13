#pragma once

#include <windows.h>

#include "NetworkVisualizer.h"

//#include "concurrent_hash_map.h"

#include "Utils.h"
#include "HashTree.h"
#include "AccountInfo.h"
#include "CandidateSet.h"
#include "Ledger.h"
#include "Constants.h"
//#include "Timer.h"
#include <hash_map>

#include "Simulator.h"

//#include "tbb/concurrent_hash_map.h"

//extern tbb::concurrent_hash_map<int, int> h;

static void TBB_TEST()
{
	//h.insert(7, 9);
	//h.insert(7, 10);	
}

/*timer::Timer t;

void TimerX()
{
cout << "Hello!" << endl;
}

void d()
{
t.subscribe(TimerX);
}*/

//extern TimerX::Timer tmr;// = TimerX::Timer(1000, 1000);

extern int Value;

extern Simulator sim;


//void Callback();

//void InitTimer();


namespace TNETVALIDATORCLR {

	using namespace System;
	using namespace System::ComponentModel;
	using namespace System::Collections;
	using namespace System::Windows::Forms;
	using namespace System::Data;
	using namespace System::Drawing;
	using namespace System::IO;
	using namespace System::Text;
	//using namespace System::;

	/// <summary>
	/// Summary for MainForm
	/// </summary>
	public ref class MainForm : public System::Windows::Forms::Form
	{
	public:
		MainForm(void)
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
		~MainForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::CheckBox^  checkBox_UpdateDisplay;
	protected:
	private: System::Windows::Forms::TabPage^  tabPage2;
	private: System::Windows::Forms::RichTextBox^  richTextBox_Log;
	private: System::Windows::Forms::TextBox^  textBox_Nodes;
	private: System::Windows::Forms::TableLayoutPanel^  tableLayoutPanel1;
	private: System::Windows::Forms::SplitContainer^  splitContainer1;
	private: System::Windows::Forms::TabPage^  tabPage1;
	private: System::Windows::Forms::TabControl^  tabControl1;
	private: System::Windows::Forms::TabPage^  tabPage_Visualizer;

	private: System::Windows::Forms::ToolStripStatusLabel^  StatusLabel;
	private: System::Windows::Forms::StatusStrip^  statusStrip1;
	private: System::Windows::Forms::ToolStripMenuItem^  hELPToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  simulateTreeToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  simulateVisualToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  stopToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  simulateToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  cONTROLToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  fILEToolStripMenuItem;
	private: System::Windows::Forms::MenuStrip^  menuStrip1;
	private: System::Windows::Forms::ToolStripMenuItem^  timerTestsToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  refreshToolStripMenuItem;
	private: System::Windows::Forms::Timer^  timer_UI;
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
			this->checkBox_UpdateDisplay = (gcnew System::Windows::Forms::CheckBox());
			this->tabPage2 = (gcnew System::Windows::Forms::TabPage());
			this->richTextBox_Log = (gcnew System::Windows::Forms::RichTextBox());
			this->textBox_Nodes = (gcnew System::Windows::Forms::TextBox());
			this->tableLayoutPanel1 = (gcnew System::Windows::Forms::TableLayoutPanel());
			this->splitContainer1 = (gcnew System::Windows::Forms::SplitContainer());
			this->tabPage1 = (gcnew System::Windows::Forms::TabPage());
			this->tabControl1 = (gcnew System::Windows::Forms::TabControl());
			this->tabPage_Visualizer = (gcnew System::Windows::Forms::TabPage());
			this->StatusLabel = (gcnew System::Windows::Forms::ToolStripStatusLabel());
			this->statusStrip1 = (gcnew System::Windows::Forms::StatusStrip());
			this->hELPToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->simulateTreeToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->simulateVisualToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->stopToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->simulateToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->cONTROLToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->timerTestsToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->refreshToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->fILEToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->menuStrip1 = (gcnew System::Windows::Forms::MenuStrip());
			this->timer_UI = (gcnew System::Windows::Forms::Timer(this->components));
			this->tabPage2->SuspendLayout();
			this->tableLayoutPanel1->SuspendLayout();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->splitContainer1))->BeginInit();
			this->splitContainer1->Panel1->SuspendLayout();
			this->splitContainer1->Panel2->SuspendLayout();
			this->splitContainer1->SuspendLayout();
			this->tabPage1->SuspendLayout();
			this->tabControl1->SuspendLayout();
			this->statusStrip1->SuspendLayout();
			this->menuStrip1->SuspendLayout();
			this->SuspendLayout();
			// 
			// checkBox_UpdateDisplay
			// 
			this->checkBox_UpdateDisplay->AutoSize = true;
			this->checkBox_UpdateDisplay->Checked = true;
			this->checkBox_UpdateDisplay->CheckState = System::Windows::Forms::CheckState::Checked;
			this->checkBox_UpdateDisplay->Location = System::Drawing::Point(617, 41);
			this->checkBox_UpdateDisplay->Name = L"checkBox_UpdateDisplay";
			this->checkBox_UpdateDisplay->Size = System::Drawing::Size(98, 17);
			this->checkBox_UpdateDisplay->TabIndex = 0;
			this->checkBox_UpdateDisplay->Text = L"Update Display";
			this->checkBox_UpdateDisplay->UseVisualStyleBackColor = true;
			// 
			// tabPage2
			// 
			this->tabPage2->Controls->Add(this->checkBox_UpdateDisplay);
			this->tabPage2->Location = System::Drawing::Point(4, 22);
			this->tabPage2->Name = L"tabPage2";
			this->tabPage2->Padding = System::Windows::Forms::Padding(3);
			this->tabPage2->Size = System::Drawing::Size(1018, 470);
			this->tabPage2->TabIndex = 1;
			this->tabPage2->Text = L"Settings";
			this->tabPage2->UseVisualStyleBackColor = true;
			// 
			// richTextBox_Log
			// 
			this->richTextBox_Log->BackColor = System::Drawing::Color::FromArgb(static_cast<System::Int32>(static_cast<System::Byte>(0)), static_cast<System::Int32>(static_cast<System::Byte>(0)),
				static_cast<System::Int32>(static_cast<System::Byte>(64)));
			this->richTextBox_Log->Dock = System::Windows::Forms::DockStyle::Fill;
			this->richTextBox_Log->Font = (gcnew System::Drawing::Font(L"Consolas", 9, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point,
				static_cast<System::Byte>(0)));
			this->richTextBox_Log->ForeColor = System::Drawing::Color::LawnGreen;
			this->richTextBox_Log->Location = System::Drawing::Point(0, 0);
			this->richTextBox_Log->Name = L"richTextBox_Log";
			this->richTextBox_Log->Size = System::Drawing::Size(1012, 407);
			this->richTextBox_Log->TabIndex = 0;
			this->richTextBox_Log->Text = L"";
			// 
			// textBox_Nodes
			// 
			this->textBox_Nodes->Location = System::Drawing::Point(316, 3);
			this->textBox_Nodes->Name = L"textBox_Nodes";
			this->textBox_Nodes->Size = System::Drawing::Size(100, 20);
			this->textBox_Nodes->TabIndex = 0;
			this->textBox_Nodes->Text = L"100";
			// 
			// tableLayoutPanel1
			// 
			this->tableLayoutPanel1->ColumnCount = 2;
			this->tableLayoutPanel1->ColumnStyles->Add((gcnew System::Windows::Forms::ColumnStyle(System::Windows::Forms::SizeType::Percent,
				31.01449F)));
			this->tableLayoutPanel1->ColumnStyles->Add((gcnew System::Windows::Forms::ColumnStyle(System::Windows::Forms::SizeType::Percent,
				68.9855F)));
			this->tableLayoutPanel1->Controls->Add(this->textBox_Nodes, 1, 0);
			this->tableLayoutPanel1->Dock = System::Windows::Forms::DockStyle::Fill;
			this->tableLayoutPanel1->Location = System::Drawing::Point(0, 0);
			this->tableLayoutPanel1->Name = L"tableLayoutPanel1";
			this->tableLayoutPanel1->RowCount = 2;
			this->tableLayoutPanel1->RowStyles->Add((gcnew System::Windows::Forms::RowStyle(System::Windows::Forms::SizeType::Percent, 50.13193F)));
			this->tableLayoutPanel1->RowStyles->Add((gcnew System::Windows::Forms::RowStyle(System::Windows::Forms::SizeType::Percent, 49.86807F)));
			this->tableLayoutPanel1->Size = System::Drawing::Size(1012, 53);
			this->tableLayoutPanel1->TabIndex = 0;
			// 
			// splitContainer1
			// 
			this->splitContainer1->Dock = System::Windows::Forms::DockStyle::Fill;
			this->splitContainer1->Location = System::Drawing::Point(3, 3);
			this->splitContainer1->Name = L"splitContainer1";
			this->splitContainer1->Orientation = System::Windows::Forms::Orientation::Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this->splitContainer1->Panel1->Controls->Add(this->tableLayoutPanel1);
			// 
			// splitContainer1.Panel2
			// 
			this->splitContainer1->Panel2->Controls->Add(this->richTextBox_Log);
			this->splitContainer1->Size = System::Drawing::Size(1012, 464);
			this->splitContainer1->SplitterDistance = 53;
			this->splitContainer1->TabIndex = 1;
			// 
			// tabPage1
			// 
			this->tabPage1->Controls->Add(this->splitContainer1);
			this->tabPage1->Location = System::Drawing::Point(4, 22);
			this->tabPage1->Name = L"tabPage1";
			this->tabPage1->Padding = System::Windows::Forms::Padding(3);
			this->tabPage1->Size = System::Drawing::Size(1018, 470);
			this->tabPage1->TabIndex = 0;
			this->tabPage1->Text = L"Simulate";
			this->tabPage1->UseVisualStyleBackColor = true;
			// 
			// tabControl1
			// 
			this->tabControl1->Controls->Add(this->tabPage1);
			this->tabControl1->Controls->Add(this->tabPage2);
			this->tabControl1->Controls->Add(this->tabPage_Visualizer);
			this->tabControl1->Dock = System::Windows::Forms::DockStyle::Fill;
			this->tabControl1->Location = System::Drawing::Point(0, 24);
			this->tabControl1->Name = L"tabControl1";
			this->tabControl1->SelectedIndex = 0;
			this->tabControl1->Size = System::Drawing::Size(1026, 496);
			this->tabControl1->TabIndex = 5;
			// 
			// tabPage_Visualizer
			// 
			this->tabPage_Visualizer->Location = System::Drawing::Point(4, 22);
			this->tabPage_Visualizer->Name = L"tabPage_Visualizer";
			this->tabPage_Visualizer->Padding = System::Windows::Forms::Padding(3);
			this->tabPage_Visualizer->Size = System::Drawing::Size(1018, 470);
			this->tabPage_Visualizer->TabIndex = 2;
			this->tabPage_Visualizer->Text = L"Visualizer";
			this->tabPage_Visualizer->UseVisualStyleBackColor = true;
			// 
			// StatusLabel
			// 
			this->StatusLabel->Name = L"StatusLabel";
			this->StatusLabel->Size = System::Drawing::Size(39, 17);
			this->StatusLabel->Text = L"Ready";
			// 
			// statusStrip1
			// 
			this->statusStrip1->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(1) { this->StatusLabel });
			this->statusStrip1->Location = System::Drawing::Point(0, 520);
			this->statusStrip1->Name = L"statusStrip1";
			this->statusStrip1->Size = System::Drawing::Size(1026, 22);
			this->statusStrip1->TabIndex = 4;
			this->statusStrip1->Text = L"statusStrip1";
			// 
			// hELPToolStripMenuItem
			// 
			this->hELPToolStripMenuItem->Name = L"hELPToolStripMenuItem";
			this->hELPToolStripMenuItem->Size = System::Drawing::Size(47, 20);
			this->hELPToolStripMenuItem->Text = L"HELP";
			// 
			// simulateTreeToolStripMenuItem
			// 
			this->simulateTreeToolStripMenuItem->Name = L"simulateTreeToolStripMenuItem";
			this->simulateTreeToolStripMenuItem->Size = System::Drawing::Size(200, 22);
			this->simulateTreeToolStripMenuItem->Text = L"Simulate Tree";
			// 
			// simulateVisualToolStripMenuItem
			// 
			this->simulateVisualToolStripMenuItem->Name = L"simulateVisualToolStripMenuItem";
			this->simulateVisualToolStripMenuItem->ShortcutKeys = static_cast<System::Windows::Forms::Keys>((System::Windows::Forms::Keys::Control | System::Windows::Forms::Keys::F3));
			this->simulateVisualToolStripMenuItem->Size = System::Drawing::Size(200, 22);
			this->simulateVisualToolStripMenuItem->Text = L"Simulate Visual";
			this->simulateVisualToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm::simulateVisualToolStripMenuItem_Click);
			// 
			// stopToolStripMenuItem
			// 
			this->stopToolStripMenuItem->Name = L"stopToolStripMenuItem";
			this->stopToolStripMenuItem->ShortcutKeys = static_cast<System::Windows::Forms::Keys>((System::Windows::Forms::Keys::Control | System::Windows::Forms::Keys::F2));
			this->stopToolStripMenuItem->Size = System::Drawing::Size(200, 22);
			this->stopToolStripMenuItem->Text = L"Stop";
			// 
			// simulateToolStripMenuItem
			// 
			this->simulateToolStripMenuItem->Name = L"simulateToolStripMenuItem";
			this->simulateToolStripMenuItem->ShortcutKeys = static_cast<System::Windows::Forms::Keys>((System::Windows::Forms::Keys::Control | System::Windows::Forms::Keys::F1));
			this->simulateToolStripMenuItem->Size = System::Drawing::Size(200, 22);
			this->simulateToolStripMenuItem->Text = L"Simulate";
			this->simulateToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm::simulateToolStripMenuItem_Click);
			// 
			// cONTROLToolStripMenuItem
			// 
			this->cONTROLToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(5) {
				this->simulateToolStripMenuItem,
					this->stopToolStripMenuItem, this->simulateVisualToolStripMenuItem, this->simulateTreeToolStripMenuItem, this->timerTestsToolStripMenuItem
			});
			this->cONTROLToolStripMenuItem->Name = L"cONTROLToolStripMenuItem";
			this->cONTROLToolStripMenuItem->Size = System::Drawing::Size(74, 20);
			this->cONTROLToolStripMenuItem->Text = L"CONTROL";
			// 
			// timerTestsToolStripMenuItem
			// 
			this->timerTestsToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(1) { this->refreshToolStripMenuItem });
			this->timerTestsToolStripMenuItem->Name = L"timerTestsToolStripMenuItem";
			this->timerTestsToolStripMenuItem->Size = System::Drawing::Size(200, 22);
			this->timerTestsToolStripMenuItem->Text = L"Timer Tests";
			this->timerTestsToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm::timerTestsToolStripMenuItem_Click);
			// 
			// refreshToolStripMenuItem
			// 
			this->refreshToolStripMenuItem->Name = L"refreshToolStripMenuItem";
			this->refreshToolStripMenuItem->Size = System::Drawing::Size(152, 22);
			this->refreshToolStripMenuItem->Text = L"Refresh";
			this->refreshToolStripMenuItem->Click += gcnew System::EventHandler(this, &MainForm::refreshToolStripMenuItem_Click);
			// 
			// fILEToolStripMenuItem
			// 
			this->fILEToolStripMenuItem->Name = L"fILEToolStripMenuItem";
			this->fILEToolStripMenuItem->Size = System::Drawing::Size(40, 20);
			this->fILEToolStripMenuItem->Text = L"FILE";
			// 
			// menuStrip1
			// 
			this->menuStrip1->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(3) {
				this->fILEToolStripMenuItem,
					this->cONTROLToolStripMenuItem, this->hELPToolStripMenuItem
			});
			this->menuStrip1->Location = System::Drawing::Point(0, 0);
			this->menuStrip1->Name = L"menuStrip1";
			this->menuStrip1->Size = System::Drawing::Size(1026, 24);
			this->menuStrip1->TabIndex = 3;
			this->menuStrip1->Text = L"menuStrip1";
			// 
			// timer_UI
			// 
			this->timer_UI->Enabled = true;
			this->timer_UI->Tick += gcnew System::EventHandler(this, &MainForm::timer_UI_Tick);
			// 
			// MainForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(1026, 542);
			this->Controls->Add(this->tabControl1);
			this->Controls->Add(this->statusStrip1);
			this->Controls->Add(this->menuStrip1);
			this->Name = L"MainForm";
			this->Text = L"TNet Validator - Consensus Simulator - v0.1 ";
			this->Load += gcnew System::EventHandler(this, &MainForm::MainForm_Load);
			this->tabPage2->ResumeLayout(false);
			this->tabPage2->PerformLayout();
			this->tableLayoutPanel1->ResumeLayout(false);
			this->tableLayoutPanel1->PerformLayout();
			this->splitContainer1->Panel1->ResumeLayout(false);
			this->splitContainer1->Panel2->ResumeLayout(false);
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->splitContainer1))->EndInit();
			this->splitContainer1->ResumeLayout(false);
			this->tabPage1->ResumeLayout(false);
			this->tabControl1->ResumeLayout(false);
			this->statusStrip1->ResumeLayout(false);
			this->statusStrip1->PerformLayout();
			this->menuStrip1->ResumeLayout(false);
			this->menuStrip1->PerformLayout();
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

		NetworkVisualizer^ nv = gcnew NetworkVisualizer();

		void PrintMessage(String^ msg)
		{
			richTextBox_Log->AppendText("\n" + msg);
		}

		void PrintMessage(string msg)
		{
			richTextBox_Log->AppendText("\n" + StringUtils::stops(msg));
		}

	private: System::Void simulateToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

		Ledger lgr;

		vector<AccountInfo> ais;

		int N = int::Parse(textBox_Nodes->Text);

		for (int i = 0; i < N; i++)
		{
			byte A_ID[32];

			RandomFillBytes(A_ID, 32);

			long MNY = rand();

			Hash AID = Hash(A_ID, A_ID + 32);

			AccountInfo ss = AccountInfo(AID, MNY, (std::string)"NO_NAME", 0);

			ais.push_back(ss);

			lgr.AddUserToLedger(ss);
		}

		PrintMessage("ADD Complete");

		CandidateSet cs;
		cs.GenerateTransactions(ais);
		int64_t TransactionVolume = 0;

		PrintMessage(" ===========  BEFORE ============ ");

		PrintMessage("Tree Root Hash: " + StringUtils::stops(lgr.GetRootHash().ToString()));
		int64_t T_Nodes = lgr.LedgerTree.TraverseNodes();
		PrintMessage("Nodes : " + lgr.LedgerTree.TotalNodes() + ", Total Leaves: " + lgr.LedgerTree.TotalLeaves() + ", Traversed Nodes : " + T_Nodes);
		PrintMessage("Total Money       : " + lgr.LedgerTree.TotalMoney);

		PrintMessage(" ===========  AFTER ============ ");

		LedgerOpStatistics st = lgr.ApplyTransactionToLedger(cs.getTransactions());

		PrintMessage("Tree Root Hash: " + StringUtils::stops(lgr.GetRootHash().ToString()));
		T_Nodes = lgr.LedgerTree.TraverseNodes();
		PrintMessage("Nodes : " + lgr.LedgerTree.TotalNodes() + ", Total Leaves: " + lgr.LedgerTree.TotalLeaves() + ", Traversed Nodes : " + T_Nodes);
		PrintMessage("Total Money       : " + lgr.LedgerTree.TotalMoney);
		PrintMessage("Transaction Volume: " + TransactionVolume);

		PrintMessage("Total Transactions  : " + st.TotalTransactions);
		PrintMessage("Failed Transactions : " + st.FailedTransactions);
		PrintMessage("Good Transactions   : " + st.GoodTransactions);
		PrintMessage("BlackLists          : " + st.BlackLists);

	}

	private: System::Void simulateVisualToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

		sim.StartSimulation();
		//networkVisualizer1.nodes = nodes;
		nv->Invalidate();

	}


	private: System::Void MainForm_Load(System::Object^  sender, System::EventArgs^  e) {

		nv->Name = L"NetworkVisualizer_1";
		nv->TabIndex = 0;
		nv->Dock = DockStyle::Fill;

		tabPage_Visualizer->Controls->Add(nv);
		tabPage_Visualizer->Invalidate();
	}	

	private: System::Void timerTestsToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

	
	}

	private: System::Void refreshToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

		//InitTimer();
		//PrintMessage("" + Value);
	}

	private: System::Void timer_UI_Tick(System::Object^  sender, System::EventArgs^  e) {

		if (MessageQueue.unsafe_size() > 0)
		{
			string Message;
			bool OK;
			do
			{
				OK = MessageQueue.try_pop(Message);				
				if (OK)
				{
					PrintMessage(Message);					
				}
			} while (OK);
		}

		nv->Invalidate();
		
		/*if (sim.Refreshed)
		{
			sim.Refreshed = false;
			nv->Invalidate();
		}*/

	}
    
	};
}
