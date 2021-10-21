using Microsoft.VisualStudio.Workspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.BehaviourTree
{

    //public class TreeBuilder<TRoot> where TRoot : class, ISequence<TRoot>, new()

    //{

    //    public static TRoot root;
    //    public TreeBuilder()
    //    {
    //        root = new TRoot();
    //    }
    //    public virtual SequenceBuilder<TRoot, Sequence> AddSequence
    //    {
    //        get
    //        {
    //            var seq = new Sequence();
    //            root.Hire(seq);
    //            return new SequenceBuilder<TRoot, Sequence>(this, seq);
    //        }
    //    }
    //    public virtual TickerBuilder<TRoot, TreeBuilder<TRoot>, TRoot> AddTicker
    //    {
    //        get
    //        {
    //            var tick = new Ticker();
    //            root.Hire(tick);
    //            return new TickerBuilder<TRoot, TreeBuilder<TRoot>, TRoot>(root, this, tick);
    //        }
    //    }
    //    //public LeafBuilder AddLeaf { }
    //    public TRoot BuildTree() { return root; }
    //}
    //public class SequenceBuilder<TRoot, TParent>:TreeBuilder<TRoot> where TRoot : class, ISequence<TRoot>, new()
    //                                                                   where TParent : class, TreeBuilder<TParent>, new()


    //{
    //    private TreeBuilder<TRoot> tree;
    //    private Sequence sequence;
    //    private TParent parent;
    //    public SequenceBuilder(TreeBuilder<TRoot> tree, TParent parent)
    //    {
    //        this.tree = tree;
    //        this.sequence = sequence;
    //        this.parent = parent;            
    //    }
    //    public override SequenceBuilder<TRoot, TreeBuilder<TRoot>, TRoot> AddSequence
    //    {
    //        get
    //        {
    //            var seq = new Sequence();
    //            sequence.Hire(seq);
    //            return new SequenceBuilder<TRoot, TreeBuilder<TRoot>, TRoot>(root, this, seq);
    //        }
    //    }
    //    public override TickerBuilder<TRoot, TreeBuilder<TRoot>, TRoot> AddTicker
    //    {
    //        get
    //        {
    //            var tick = new Ticker();
    //            root.Hire(tick);
    //            return new TickerBuilder<TRoot, TreeBuilder<TRoot>, TRoot>(root, this, tick);
    //        }
    //    }
    //    public TParent Build() => parent;        
    //}
    //public class TickerBuilder<TRoot, TParent, T> : TreeBuilder<TRoot> where TRoot : class, ISequence<TRoot>, new()
    //                                                                   where T : class, ISequence<T>, new()
    //                                                                   where TParent : TreeBuilder<T>
    //{
    //    private TRoot root;
    //    private Ticker ticker;
    //    private TParent parent;
    //    public TickerBuilder(TRoot root, TParent parent, Ticker ticker)
    //    {
    //        this.root = root;
    //        this.ticker = ticker;
    //        this.parent = parent;
    //    }
    //    public override SequenceBuilder<TRoot, TreeBuilder<TRoot>, TRoot> AddSequence
    //    {
    //        get
    //        {
    //            var seq = new Sequence();
    //            ticker.Hire(seq);
    //            return new SequenceBuilder<TRoot, TreeBuilder<TRoot>, TRoot>(root, this, seq);
    //        }
    //    }
    //    public TParent Build() => parent;

    //}
    //public class LeafBuilder
    //{

    //}
}
