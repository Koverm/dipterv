using PetriNetBase;
using PetriTool;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Threading;
using Stubbornforms;

[AddinAuthor("Kövér Márton"), ToolVersion("1.5"),
IncludeInPublicRelease]
public class DummyAddin : IPDNPlugin
{
    private PDNAppDescriptor appDesc = null;

    static public string filePath = @"D:\egyetem\dipterv1\result.dt";

    public void Initialize(PDNAppDescriptor appDesc)
    {
        this.appDesc = appDesc;
        appDesc.AddPluginMenuItem("Stubborn set\\Places", Foo);
        //appDesc.AddPluginMenuItem("Stubborn set\\Transitions", Bar);
    }
    private void Foo(object sender, EventArgs e)
    {
        PetriNet pn = appDesc.CurrentPetriNet;

        if (pn != null)
        {
            NetState.initCounter();
            Thread t = new Thread(new ParameterizedThreadStart(ProcessingThread));
            t.Start(pn);
            string message = "Result: " + filePath;
            string title = "Places";
            MessageBox.Show(message, title);
        }
        else
            MessageBox.Show("Please open or create a Petri net", "No active Petri net", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public static void ProcessingThread(object net) {

        PetriNet pn = (PetriNet)net;
        
        List<Place> places = pn.GetPlaces();

        NetState ns = new NetState(places);

        List<Transition> tranzitions = pn.GetTransitions();
        List<NetTransition> netTransitions = new List<NetTransition>();

        Dictionary<int[], NetState> visitedStates = new Dictionary<int[], NetState>(new IntArrayEqualityComparer());

        visitedStates.Add(ns.States, ns);

        foreach (var tran in tranzitions)
        {
            netTransitions.Add(new NetTransition(places, tran));
        }

        Stack<NetState> stateStack = new Stack<NetState>();
        stateStack.Push(ns);

        Queue<NetTransition> fireableTransitions = new Queue<NetTransition>();

        foreach (var trans in netTransitions)
        {
            if (ns.fireable(trans))
            {
                fireableTransitions.Enqueue(trans);
            }
        }

        Stack<Queue<NetTransition>> transitionStack = new Stack<Queue<NetTransition>>();
        transitionStack.Push(fireableTransitions);

        while (stateStack.Count > 0 && transitionStack.Count > 0)
        {
            NetState stateTmp = stateStack.Pop();
            Queue<NetTransition> transitionsTmp = transitionStack.Pop();

            if (transitionsTmp.Count > 0)
            {
                NetTransition netTransitionTmp = transitionsTmp.Dequeue();

                stateStack.Push(stateTmp);
                transitionStack.Push(transitionsTmp);

                NetState newState = new NetState(stateTmp, netTransitionTmp);

                if (!visitedStates.ContainsKey(newState.States))
                {
                    visitedStates.Add(newState.States, newState);
                    

                    Queue<NetTransition> tmpQueue = new Queue<NetTransition>();

                    foreach (var trans in netTransitions)
                    {
                        if (newState.fireable(trans))
                        {
                            tmpQueue.Enqueue(trans);
                        }
                    }

                    stateStack.Push(newState);
                    transitionStack.Push(tmpQueue);
                }
                visitedStates[stateTmp.States].Neighbours.Add(visitedStates[newState.States]);
            }
        }

        using (System.IO.StreamWriter file =
        new System.IO.StreamWriter(filePath))
        {
            if (visitedStates.Count <= 1048576)
            {
                file.WriteLine("digraph g{");
                foreach (KeyValuePair<int[], NetState> entry in visitedStates)
                {
                    foreach (var nstate in entry.Value.Neighbours)
                    {
                        file.WriteLine(entry.Value.toString() + "->" + nstate.toString() + ";");
                    }
                }
                file.Write("}");
            }
            else
            {
                file.WriteLine("Number of states: " + visitedStates.Count);
            }

        }
        string message = "Ready";
        string title = "Processing ready";
        MessageBox.Show(message, title);

    }
    private void Bar(object sender, EventArgs e)
    {
        PetriNet pn = appDesc.CurrentPetriNet;

        if (pn != null)
        {
            List<Transition> tranzitions = pn.GetTransitions();

            string message = "";

            foreach (var tr in tranzitions)
            {
                message += tr.Name + "\n";
            }

            string title = "Tranzitions";
            MessageBox.Show(message, title);
        }
        else
            MessageBox.Show("Please open or create a Petri net", "No active Petri net", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}