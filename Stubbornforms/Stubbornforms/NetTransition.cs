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

        public List<NetTransition> F1(List<NetTransition> transitions, NetState state){
            List<NetTransition> result = new List<NetTransition>();

            foreach (var item in transitions)
            {

            }

            return result;
        }
        public List<NetTransition> F2(List<NetTransition> transitions, NetState state)
        {

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
