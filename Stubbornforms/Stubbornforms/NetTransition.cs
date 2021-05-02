using PetriNetBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stubbornforms
{

    public class NetTransition
    {
        public int[] inEdges { get; private set; }
        public int[] outEdges { get; private set; }
        public string Name { get; private set; }

        public NetTransition(List<Place> places, Transition transition)
        {
            Name = transition.Name;

            inEdges = new int[places.Count];
            outEdges = new int[places.Count];

            for (int i = 0; i < inEdges.Length; i++)
            {
                inEdges[i] = 0;
                outEdges[i] = 0;
            }

            List<Edge> inputEdges = transition.InputEdges;

            foreach (var edge in inputEdges)
            {
                int i = places.IndexOf((Place)edge.Source);
                if (i > -1) {
                    inEdges[i] = edge.Weight;
                }

            }

            List<Edge> outputEdges = transition.OutputEdges;

            foreach (var edge in outputEdges)
            {
                int i = places.IndexOf((Place)edge.Target);
                if (i > -1)
                {
                    outEdges[i] = edge.Weight;
                }

            }

        }

        public List<NetTransition> slowF1(List<NetTransition> transitions, NetState state)
        {
            List<NetTransition> result = new List<NetTransition>();

            int place = -1;
            for (int i = 0; i < state.States.Length; i++)
            {
                if (this.inEdges[i] > 0 && state.States[i] < this.inEdges[i])
                {
                    place = i;
                    break;
                }
            }

            if (place > -1)
            {
                foreach (var item in transitions)
                {
                    if (item.outEdges[place] > 0)
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        public List<NetTransition> slowF2(List<NetTransition> transitions, NetState state)
        {
            List<NetTransition> result = new List<NetTransition>();

            foreach (var item in transitions)
            {
                for (int i = 0; i < state.States.Length; i++)
                {
                    if (this.inEdges[i] > 0 && item.inEdges[i] > 0)
                    {
                        result.Add(item);
                        break;
                    }
                }
            }

            return result;
        }

        public List<NetTransition> F1(List<NetTransition> transitions, NetState state)
        {
            List<NetTransition> result = new List<NetTransition>();

            foreach (var item in transitions)
            {
                for (int i = 0; i < state.States.Length; i++)
                {
                    if (this.inEdges[i] > 0)
                    {
                        if (state.States[i] < this.inEdges[i] &&
                            item.inEdges[i] < item.outEdges[i] &&
                            item.inEdges[i] <= state.States[i])
                        {
                            result.Add(item);
                            break;
                        }
                    }
                }
                if (result.Count >= 1) {
                    break;
                }
            }

            return result;
        }
        public List<NetTransition> F2(List<NetTransition> transitions, NetState state)
        {
            List<NetTransition> result = new List<NetTransition>();
            List<int> p = new List<int>();


            for (int i = 0; i < state.States.Length; i++)
            {
                if (this.inEdges[i] > 0)
                {
                    p.Add(i);
                }
            }

            foreach (var item in transitions)
            {
                int i = 0;
                while (i < p.Count && (Math.Min(this.outEdges[p[i]], item.outEdges[p[i]]) < Math.Min(this.inEdges[p[i]], item.inEdges[p[i]]) ||
                            Math.Min(this.outEdges[p[i]], item.inEdges[p[i]]) < Math.Min(this.inEdges[p[i]], item.outEdges[p[i]])))
                {
                    i++;
                }
                if (i >= p.Count)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public string InEdgesToString() {
            string message = "";

            for (int i = 0; i < inEdges.Length - 1; i++)
            {
                message += inEdges[i] + ",";
            }
            message += inEdges[inEdges.Length - 1];

            return message;
        }
        public string OutEdgesToString()
        {
            string message = "";

            for (int i = 0; i < outEdges.Length - 1; i++)
            {
                message += outEdges[i] + ",";
            }
            message += outEdges[outEdges.Length - 1];

            return message;
        }
    }
}
