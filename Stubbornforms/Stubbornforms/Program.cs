using PetriNetBase;
using PetriTool;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Threading;
using Stubbornforms;
using System.Diagnostics;

[AddinAuthor("Kövér Márton"), ToolVersion("1.5"),
IncludeInPublicRelease]
public class StateSpace : IPDNPlugin
{
    private PDNAppDescriptor appDesc = null;

    public void Initialize(PDNAppDescriptor appDesc)
    {
        this.appDesc = appDesc;
        appDesc.AddPluginMenuItem("State space\\Full state space", Fss);
        appDesc.AddPluginMenuItem("State space\\D1,D2 reduced state space slow", D1d2rssSlow);
        appDesc.AddPluginMenuItem("State space\\D1,D2 reduced state space", D1d2rss);
        appDesc.AddPluginMenuItem("State space\\D1,D2 reduced state space improved", D1d2imp);
        //appDesc.AddPluginMenuItem("State space\\StubbornTest", Bar);
    }
    private void Fss(object sender, EventArgs e)
    {
        PetriNet pn = appDesc.CurrentPetriNet;

        if (pn != null)
        {
            NetState.initCounter();
            Thread t = new Thread(new ParameterizedThreadStart(FssProcessingThread));
            t.Start(pn);
        }
        else
            MessageBox.Show("Please open or create a Petri net", "No active Petri net", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public static void FssProcessingThread(object net) {

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

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

        stopWatch.Stop();

        writeResultToFile(visitedStates,stopWatch.ElapsedTicks);

    }


    private void D1d2rssSlow(object sender, EventArgs e)
    {
        PetriNet pn = appDesc.CurrentPetriNet;

        if (pn != null)
        {
            NetState.initCounter();
            Thread t = new Thread(new ParameterizedThreadStart(D1d2rssProcessingThreadSlow));
            t.Start(pn);
        }
        else
            MessageBox.Show("Please open or create a Petri net", "No active Petri net", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void D1d2rss(object sender, EventArgs e)
    {
        PetriNet pn = appDesc.CurrentPetriNet;

        if (pn != null)
        {
            NetState.initCounter();
            Thread t = new Thread(new ParameterizedThreadStart(D1d2rssProcessingThread));
            t.Start(pn);
        }
        else
            MessageBox.Show("Please open or create a Petri net", "No active Petri net", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void D1d2imp(object sender, EventArgs e)
    {
        PetriNet pn = appDesc.CurrentPetriNet;

        if (pn != null)
        {
            NetState.initCounter();
            Thread t = new Thread(new ParameterizedThreadStart(D1d2impProcessingThread));
            t.Start(pn);
        }
        else
            MessageBox.Show("Please open or create a Petri net", "No active Petri net", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public static void D1d2rssProcessingThreadSlow(object net)
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

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

        Queue<NetTransition> stubTransitions = getStubbornsetSlow(netTransitions, ns);

        Stack<Queue<NetTransition>> transitionStack = new Stack<Queue<NetTransition>>();
        transitionStack.Push(stubTransitions);

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

                    tmpQueue = getStubbornsetSlow(netTransitions, newState);

                    stateStack.Push(newState);
                    transitionStack.Push(tmpQueue);
                }
                visitedStates[stateTmp.States].Neighbours.Add(visitedStates[newState.States]);
            }
        }

        stopWatch.Stop();

        writeResultToFile(visitedStates, stopWatch.ElapsedTicks);

    }
    public static void D1d2rssProcessingThread(object net)
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

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

        Queue<NetTransition> stubTransitions = getStubbornset(netTransitions, ns);

        Stack<Queue<NetTransition>> transitionStack = new Stack<Queue<NetTransition>>();
        transitionStack.Push(stubTransitions);

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

                    tmpQueue = getStubbornset(netTransitions, newState);

                    stateStack.Push(newState);
                    transitionStack.Push(tmpQueue);
                }
                visitedStates[stateTmp.States].Neighbours.Add(visitedStates[newState.States]);
            }
        }

        stopWatch.Stop();

        writeResultToFile(visitedStates, stopWatch.ElapsedTicks);

    }

    public static void D1d2impProcessingThread(object net)
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

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

        Queue<NetTransition> stubTransitions = getStubbornsetImp(netTransitions, ns);

        Stack<Queue<NetTransition>> transitionStack = new Stack<Queue<NetTransition>>();
        transitionStack.Push(stubTransitions);

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

                    tmpQueue = getStubbornsetImp(netTransitions, newState);

                    stateStack.Push(newState);
                    transitionStack.Push(tmpQueue);
                }
                visitedStates[stateTmp.States].Neighbours.Add(visitedStates[newState.States]);
            }
        }

        stopWatch.Stop();

        writeResultToFile(visitedStates, stopWatch.ElapsedTicks);

    }

    public void Bar(object sender, EventArgs e)
    {
        PetriNet pn = appDesc.CurrentPetriNet;
        if (pn != null)
        {
            List<Transition> tranzitions = pn.GetTransitions();
            List<NetTransition> netTransitions = new List<NetTransition>();
            List<Place> places = pn.GetPlaces();
            NetState ns = new NetState(places);

            foreach (var tran in tranzitions)
            {
                netTransitions.Add(new NetTransition(places, tran));
            }

            string message = "";
            Queue<NetTransition> stubborn = getStubbornsetSlow(netTransitions, ns);
            foreach (var tr in stubborn)
            {
                message += tr.Name + "\n";
            }
            string title = "Tranzitions";
            MessageBox.Show(message, title);
        }
        else
            MessageBox.Show("Please open or create a Petri net", "No active Petri net", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public static Queue<NetTransition> getStubbornsetSlow(List<NetTransition> transitions, NetState state)
    {
        List<NetTransition> fireableTransitions = new List<NetTransition>();
        foreach (var trans in transitions)
        {
            if (state.fireable(trans))
            {
                fireableTransitions.Add(trans);
            }
        }

        List<NetTransition> Ts = new List<NetTransition>();
        NetTransition Tk = transitions[0];
        int tmp = Int32.MaxValue;

        foreach (var item in fireableTransitions)
        {
            List<NetTransition> Ttmp = new List<NetTransition>();
            Ttmp.Add(item);
            foreach (var item2 in fireableTransitions)
            {
                if (!Ttmp.Contains(item2))
                {
                    NetState NsTmp = new NetState(state, item2);
                    if (!NsTmp.fireable(item))
                    {
                        Ttmp.Add(item2);
                    }
                }
            }
            if (Ttmp.Count < tmp)
            {
                tmp = Ttmp.Count;
                Tk = item;
            }
        }
        Ts.Add(Tk);

        int i = 0;

        while (i < Ts.Count)
        {
            List<NetTransition> tmpList = new List<NetTransition>();
            if (!state.fireable(Ts[i]))
            {
                tmpList = Ts[i].slowF1(transitions, state);
            }
            else {
                tmpList = Ts[i].slowF2(transitions, state);
            }
            foreach (var item in tmpList)
            {
                if (!Ts.Contains(item)) {
                    Ts.Add(item);
                }
            }
            ++i;
        }

        Queue<NetTransition> result = new Queue<NetTransition>();

        foreach (var item in Ts)
        {
            if (state.fireable(item))
            {
                result.Enqueue(item);
            }
        }
        return result;
    }

    public static Queue<NetTransition> getStubbornset(List<NetTransition> transitions, NetState state)
    {
        List<NetTransition> fireableTransitions = new List<NetTransition>();
        foreach (var trans in transitions)
        {
            if (state.fireable(trans))
            {
                fireableTransitions.Add(trans);
            }
        }

        List<NetTransition> Ts = new List<NetTransition>();
        NetTransition Tk = transitions[0];
        int tmp = Int32.MaxValue;

        foreach (var item in fireableTransitions)
        {
            List<NetTransition> Ttmp = new List<NetTransition>();
            Ttmp.Add(item);

            foreach (var item2 in fireableTransitions)
            {
                if (!Ttmp.Contains(item2))
                {
                    for (int j = 0; j < state.States.Length; j++)
                    {

                        if (item.inEdges[j] > 0)
                            if( item2.outEdges[j] < Math.Min(item2.inEdges[j], item.inEdges[j]))
                            {
                                Ttmp.Add(item2);
                                break;
                            }
                    }
                }
            }
            if (Ttmp.Count < tmp)
            {
                tmp = Ttmp.Count;
                Tk = item;
            }
        }
        Ts.Add(Tk);

        int i = 0;

        while (i < Ts.Count)
        {
            List<NetTransition> tmpList = new List<NetTransition>();
            if (!state.fireable(Ts[i]))
            {
                tmpList = Ts[i].F1(transitions, state);
            }
            else
            {
                tmpList = Ts[i].F2(transitions, state);
            }
            foreach (var item in tmpList)
            {
                if (!Ts.Contains(item))
                {
                    Ts.Add(item);
                }
            }
            ++i;
        }

        Queue<NetTransition> result = new Queue<NetTransition>();

        foreach (var item in Ts)
        {
            if (state.fireable(item))
            {
                result.Enqueue(item);
            }
        }
        return result;
    }
    public static Queue<NetTransition> getStubbornsetImp(List<NetTransition> transitions, NetState state)
    {
        List<NetTransition> fireableTransitions = new List<NetTransition>();
        foreach (var trans in transitions)
        {
            if (state.fireable(trans))
            {
                fireableTransitions.Add(trans);
            }
        }

        List<NetTransition> AviableTransitions = new List<NetTransition>(transitions);
        List<NetTransition> Ts = new List<NetTransition>();
        Ts.Add(AviableTransitions[0]);

        int tmp = Int32.MaxValue;

        foreach (var item in fireableTransitions)
        {
            List<NetTransition> Ttmp = new List<NetTransition>();
            Ttmp.Add(item);

            foreach (var item2 in fireableTransitions)
            {
                if (!Ttmp.Contains(item2))
                {
                    for (int j = 0; j < state.States.Length; j++)
                    {

                        if (item.inEdges[j] > 0)
                            if (item2.outEdges[j] < Math.Min(item2.inEdges[j], item.inEdges[j]))
                            {
                                Ttmp.Add(item2);
                                break;
                            }
                    }
                }
            }
            if (Ttmp.Count < tmp)
            {
                tmp = Ttmp.Count;
                Ts = Ttmp;
            }
        }

        foreach (var item in Ts)
        {
            AviableTransitions.Remove(item);
        }

        int i = 0;

        while (i < Ts.Count)
        {
            List<NetTransition> tmpList = new List<NetTransition>();
            if (!state.fireable(Ts[i]))
            {
                tmpList = Ts[i].F1(AviableTransitions, state);
            }
            else
            {
                tmpList = Ts[i].F22(AviableTransitions, state);
            }
            foreach (var item in tmpList)
            {
                if (!Ts.Contains(item))
                {
                    Ts.Add(item);
                    AviableTransitions.Remove(item);
                }
            }
            ++i;
        }

        Queue<NetTransition> result = new Queue<NetTransition>();

        foreach (var item in Ts)
        {
            if (state.fireable(item))
            {
                result.Enqueue(item);
            }
        }
        return result;
    }

    private static void writeResultToFile(Dictionary<int[], NetState> visitedStates, long elapsedTime)
    {
        //SaveFileDialog saveFileDialog1 = new SaveFileDialog();
        //saveFileDialog1.Filter = "dot files|*.dt|all files (*.*)|*.*";
        //saveFileDialog1.Title = "Save";
        //saveFileDialog1.ShowDialog();

        string filePath = "result" + Guid.NewGuid() + ".dt";

        
        // Save document
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
                file.WriteLine("#Number of states: " + visitedStates.Count);
                file.WriteLine("#Elapsedtime in ticks: " + elapsedTime);
                file.Write("}");
            }
            else
            {
                file.WriteLine("#Number of states: " + visitedStates.Count);
                file.WriteLine("#Elapsedtime in ticks: " + elapsedTime);
            }
        }

        MessageBox.Show("See result in: "+filePath,"Ready");

    }
}