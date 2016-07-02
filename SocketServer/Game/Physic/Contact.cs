using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyLib 
{
    public class ContactResult
    {
        public Shape shape1;
        public Shape shape2;
        public Manifold manifold;
    }

    public delegate Contact ContactCreateFunc(Shape s1, Shape s2);

    public struct ContactRegister
    {
        public ContactCreateFunc create;
    }

    public abstract class Contact
    {
        private static ContactRegister[][] registers = new ContactRegister[(int)ShapeType.ShapeCount][];

        private static void AddType(ContactCreateFunc create, ShapeType s1, ShapeType s2)
        {
            if (registers[(int) s1] == null)
            {
                registers[(int)s1] = new ContactRegister[(int)ShapeType.ShapeCount];
            }
            registers[(int) s1][(int) s2].create = create;
        }
        public static void InitializeRegister()
        {
            AddType(CircleContact.Create, ShapeType.CircleShape, ShapeType.CircleShape);
            AddType(PolyAndCircleContact.Create, ShapeType.PolygonShape, ShapeType.CircleShape);
            AddType(PolyAndCircleContact.CreateReverse, ShapeType.CircleShape, ShapeType.PolygonShape);
        }

        public static Contact Create(Shape s1, Shape s2)
        {
            var t1 = s1._type;
            var t2 = s2._type;
            var createFunc = registers[(int)t1][(int)t2].create;
            return createFunc(s1, s2);
        }

        public abstract bool Overlap();

        protected Shape s1;
        protected Shape s2;
        public Contact(Shape s1, Shape s2)
        {
            this.s1 = s1;
            this.s2 = s2;
        }
    }


    public class CircleContact : Contact
    {
        public CircleContact(Shape s1, Shape s2)
            :base(s1, s2)
        {
        }
        public override bool Overlap()
        {
            var m0 = new Manifold();
            Collision.CollideCircles(ref m0, (Circle)s1, s1._body.xform, (Circle)s2, s2._body.xform);
            return m0.PointCount > 0;
        }

        new public static Contact Create(Shape s1, Shape s2)
        {
            return new CircleContact(s1, s2);
        }
    }

    public class PolyAndCircleContact : Contact
    {
        public PolyAndCircleContact(Shape s1, Shape s2) : base(s1, s2)
        {
            
        }

        public override bool Overlap()
        {
            var m0 = new Manifold();
            Collision.CollidePolygonAndCircle(ref m0, (PolygonShape)s1, s1._body.xform, (Circle)s2, s2._body.xform);
            return m0.PointCount > 0;
        }

        new public static Contact Create(Shape s1, Shape s2)
        {
            return new PolyAndCircleContact(s1, s2);
        }

        public static Contact CreateReverse(Shape s1, Shape s2)
        {
            return new PolyAndCircleContact(s2, s1);
        }
    }
}
