﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Encog.Neural.Networks.Synapse;
using log4net;
using Encog.Neural.Data;
using Encog.Neural.Networks.Layers;

namespace Encog.Neural.Networks.Logic
{
    /// <summary>
    /// Provides the neural logic for an Feedforward type network.  See FeedforwardPattern
    /// for more information on this type of network.
    /// </summary>
    [Serializable]
    public class FeedforwardLogic : INeuralLogic
    {
        /// <summary>
        /// The logging object.
        /// </summary>
        [NonSerialized]
        private static readonly ILog logger = LogManager.GetLogger(typeof(FeedforwardLogic));

        /// <summary>
        /// The network to use.
        /// </summary>
        private BasicNetwork network;

        /// <summary>
        /// Compute the output for a given input to the neural network. This method
        /// provides a parameter to specify an output holder to use.  This holder
        /// allows propagation training to track the output from each layer.
        /// If you do not need this holder pass null, or use the other 
        /// compare method.
        /// </summary>
        /// <param name="input">The input provide to the neural network.</param>
        /// <param name="useHolder">Allows a holder to be specified, this allows
        /// propagation training to check the output of each layer.</param>
        /// <returns>The results from the output neurons.</returns>
        public virtual INeuralData Compute(INeuralData input,
                 NeuralOutputHolder useHolder)
        {
            NeuralOutputHolder holder;

            ILayer inputLayer = this.network.GetLayer(BasicNetwork.TAG_INPUT);

            if (FeedforwardLogic.logger.IsDebugEnabled)
            {
                FeedforwardLogic.logger.Debug("Pattern " + input.ToString()
                    + " presented to neural network");
            }

            if (useHolder == null)
            {
                holder = new NeuralOutputHolder();
            }
            else
            {
                holder = useHolder;
            }

            this.Network.CheckInputSize(input);
            Compute(holder, inputLayer, input, null);
            return holder.Output;
        }

        /// <summary>
        /// Internal computation method for a single layer.  This is called, 
        /// as the neural network processes.
        /// </summary>
        /// <param name="holder">The output holder.</param>
        /// <param name="layer">The layer to process.</param>
        /// <param name="input">The input to this layer.</param>
        /// <param name="source">The source synapse.</param>
        private void Compute(NeuralOutputHolder holder, ILayer layer,
                 INeuralData input, ISynapse source)
        {

            if (FeedforwardLogic.logger.IsDebugEnabled)
            {
                FeedforwardLogic.logger.Debug("Processing layer: "
                    + layer.ToString()
                    + ", input= "
                    + input.ToString());
            }

            // typically used to process any recurrent layers that feed into this
            // layer.
            PreprocessLayer(layer, input, source);

            foreach (ISynapse synapse in layer.Next)
            {
                if (!holder.Result.ContainsKey(synapse))
                {
                    if (FeedforwardLogic.logger.IsDebugEnabled)
                    {
                        FeedforwardLogic.logger.Debug("Processing synapse: " + synapse.ToString());
                    }
                    INeuralData pattern = synapse.Compute(input);
                    pattern = synapse.ToLayer.Compute(pattern);
                    synapse.ToLayer.Process(pattern);
                    holder.Result[synapse] = input;
                    Compute(holder, synapse.ToLayer, pattern, synapse);

                    ILayer outputLayer = this.network.GetLayer(BasicNetwork.TAG_OUTPUT);

                    // Is this the output from the entire network?
                    if (synapse.ToLayer == outputLayer)
                    {
                        holder.Output = pattern;
                    }
                }
            }
        }

        /// <summary>
        /// Setup the network logic, read parameters from the network.
        /// </summary>
        /// <param name="network">The network that this logic class belongs to.</param>
        public virtual void Init(BasicNetwork network)
        {
            this.network = network;
        }

        /// <summary>
        /// The network in use.
        /// </summary>
        public BasicNetwork Network
        {
            get
            {
                return network;
            }
        }

        /// <summary>
        /// Can be overridden by subclasses.  Usually used to implement recurrent 
        /// layers. 
        /// </summary>
        /// <param name="layer">The layer to process.</param>
        /// <param name="input">The input to this layer.</param>
        /// <param name="source">The source from this layer.</param>
        virtual public void PreprocessLayer(ILayer layer, INeuralData input, ISynapse source)
        {
            // nothing to do		
        }
    }
}
