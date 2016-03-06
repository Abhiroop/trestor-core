//
// SimulationPage.xaml.cpp
// Implementation of the SimulationPage class
//

#include "pch.h"
#include "SimulationPage.xaml.h"

#include "Sources\Utils.h"
#include "Sources\HashTree.h"
#include "Sources\AccountInfo.h"
#include "Sources\CandidateSet.h"
#include "Sources\Ledger.h"

#include <hash_map>

using namespace TNET_UI;

using namespace Platform;
using namespace Platform::Collections;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI;
using namespace Windows::UI::Core;
using namespace Windows::UI::Input::Inking;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;
using namespace Windows::UI::Xaml::Shapes;
using namespace Windows::UI::Xaml::Shapes;

SimulationPage::SimulationPage()
{
	InitializeComponent();
}

void TNET_UI::SimulationPage::PrintMessage(String^ msg)
{
	textBlock_Log->Text += "\n" + msg;
}

void TNET_UI::SimulationPage::button_StartSimulation_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	Line^ myLine = ref new Line();
	myLine->Stroke = ref new SolidColorBrush(Colors::Blue); // Brushes ::LightSteelBlue;
	myLine->X1 = 1;
	myLine->X2 = 50;
	myLine->Y1 = 1;
	myLine->Y2 = 50;
	myLine->HorizontalAlignment = Windows::UI::Xaml::HorizontalAlignment::Left;
	myLine->VerticalAlignment = Windows::UI::Xaml::VerticalAlignment::Center;
	myLine->StrokeThickness = 2;

	NetworkCanvas->Children->Append(myLine);

	HashTree< AccountInfo > ht;

	//lfh.loadledger();

	byte PKS[32] = { 128, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
		0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };

	Hash h_PKS = Hash(PKS, PKS + 32);

	uint64_t Taka = 0;

	//for (int k = 0; k < 4; k++)
	//for (int j = 0; j < 256; j++)
	for (int i = 0; i < 10; i++)
	{
		byte PK[32] = { i, 2, 3, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
			0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };

		//long TAKA = 1000;

		Hash h = Hash(PK, PK + 32);

		h_PKS = h;

		string name = "a";
		int64_t lastdate = 0;
		AccountInfo si = AccountInfo(h, Taka++, name, lastdate);

		Hash HH = si.GetHash();

		textBlock_Log->Text += "\n" + StringUtils::stops(HH.ToString()) + "\n";

		//Hash h3 = ht.GetRootHash();

		ht.AddUpdate(si);

	}	

	AccountInfo hh;
	ht.GetNodeData(h_PKS, hh);

	PrintMessage("\n ROOT HASH: " + StringUtils::stops(ht.GetRootHash().ToString()));

	PrintMessage("\n TOTAL MONEY ITERS: " + Taka);

	PrintMessage("\n MONEY: " + hh.Money);

	ht.TraverseNodes();

	PrintMessage("\n TOTAL NODES: " + (int64_t)ht.TotalNodes());
	PrintMessage("\n TOTAL MONEY: " + (int64_t)ht.TotalMoney);
}


void TNET_UI::SimulationPage::TestTX()
{
	Ledger lgr;

	//HashTree account_tree = new HashTree();

	//Stopwatch sw = new Stopwatch();

	vector<AccountInfo> ais;

	//Dictionary<Hash, AccountInfo> ais = new Dictionary<Hash, AccountInfo>();

	int N = StringUtils::ToInt(textBox_Nodes->Text);

	for (int i = 0; i < N; i++)
	{
		byte A_ID[32];

		RandomFillBytes(A_ID, 32);

		long MNY = rand();

		Hash AID = Hash(A_ID, A_ID + 32);

		AccountInfo ss = AccountInfo(AID, MNY, (std::string)"NO_NAME", 0);

		ais.push_back(ss);

		lgr.AddUserToLedger(ss);

		//AccountInfo ai = (AccountInfo)account_tree.GetNodeData(ss.GetID());
		//DisplayUtils.Display(ai.Money + "\n");
	}

	PrintMessage("ADD Complete");

	//Application.DoEvents();
	//AccountInfo sss = new AccountInfo(AID, 1000);
	//account_tree.AddUpdate(sss);

	//sw.Start();

	//account_tree.TotalMoney = 0;

	//int64_t val = 0; val = account_tree.TraverseNodes();

	//////////////////

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

	/////////////////

	//sw.Stop();
	//DisplayUtils.Display("Time: " + sw.Elapsed);
}

void TNET_UI::SimulationPage::button_TransactionTest_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	TestTX();
}


void TNET_UI::SimulationPage::AritaTest_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	textBlock_Log->Text += "\nHELLO !!!";
}
