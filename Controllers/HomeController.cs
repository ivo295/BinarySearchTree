using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BinarySearchTreeNamesDB.Models;

/* Course: ACST 3330    
 * Section: W01
 * Name: Ivo Simeonov
 * Proffessor: Prof.Shaw    
 * Assignment #: Mod 8 Assignment 2
 */

namespace BinarySearchTreeNamesDB.Controllers
{
    public class HomeController : Controller
    {
        // DB link for TreeNodes Table
        public TreeDBEntities db1 = new TreeDBEntities();

        // DB link for NameRoots Table
        public TreeDBEntities1 db2 = new TreeDBEntities1();


        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to the Binary Tree Name Database";
            ViewBag.TheNode = "";
            Session["OrderType"] = "In Order";
            ViewBag.OrderType = Session["OrderType"];
            ViewBag.OrderList = InOrderList();
            ViewBag.Count = ViewBag.OrderList.Count;

            return View();
        }

        [HttpPost]
        public ActionResult Index(string button, FormCollection form)
        {
            ViewBag.Message = "Welcome to the Binary Tree Name Database";
            ViewBag.theNode = "";

            
            if (button == "Add" && form["Node"] != "")
            {

                if (Search(form["Node"]) == false)
                {
                    Insert(form["Node"]);

                }


            }
            else if (button == "Remove" && form["Node"] != "")
            {

                if (Search(form["Node"]) == true)
                {
                    Delete(form["Node"]);
                    ViewBag.theNode = form["Node"];
                }
            }
            else if (button == "In Order")
            {
                Session["OrderType"] = "In Order";
            }
            else if (button == "Pre Order")
            {
                Session["OrderType"] = "Pre Order";
            }
            else if (button == "Post Order")
            {
                Session["OrderType"] = "Post Order";
            }
            ViewBag.OrderType = Session["OrderType"];
            if (ViewBag.OrderType == "In Order")
                ViewBag.OrderList = InOrderList();
            else if (ViewBag.OrderType == "Pre Order")
                ViewBag.OrderList = PreOrderList();
            else
                ViewBag.OrderList = PostOrderList();


            ViewBag.Count = ViewBag.OrderList.Count;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About the Binary Tree Name Database";
            ViewBag.Message += "<br />Webmaster Ivo Simeonov";

            return View();
        }


        /*********************************/
        /*                               */
        /* Database Manipulation Methods */
        /*                               */
        /*********************************/

        /* Get the NameRoot ID */
        public int GetNameRootID()
        {
            int rootID = -1;
            var result = from n in db2.NameRoots select n;
            if (result.Count() > 0)
            {
                NameRoot link = result.FirstOrDefault();
                rootID = link.RootID;
            }
            return rootID;
        }

        /* Set the NameRoot to given ID */
        public void SetNameRootID(int ID)
        {
            var result = from n in db2.NameRoots select n;
            if (result.Count() > 0)
            {
                NameRoot link = result.FirstOrDefault();
                link.RootID = ID;
                db2.SaveChanges();
            }
            else
            {
                NameRoot link = new NameRoot();
                link.RootID = ID;
                db2.NameRoots.Add(link);
                db2.SaveChanges();
            }

        }

        /* Get TreeNode at node given by ID */
        public TreeNode GetTreeNode(int ID)
        {
            TreeNode node = null;
            if (ID != -1)
            {
                var result = from t in db1.TreeNodes
                             where (t.ID == ID)
                             select t;
                if (result.Count() > 0)
                {
                    node = result.FirstOrDefault();
                }
            }
            return node;
        }

        /* Get element at node given by ID */
        /* and return the IDs of the left and right nodes */
        public string GetElement(int ID, out int left, out int right)
        {
            string element = "";
            left = right = -1;
            if (ID != -1)
            {
                var result = from t in db1.TreeNodes
                             where (t.ID == ID)
                             select t;
                if (result.Count() > 0)
                {
                    TreeNode node = result.FirstOrDefault();
                    element = node.Element;
                    left = node.Left;
                    right = node.Right;
                }
            }
            return element;
        }

        /* Create a new TreeNode with empty leaves */
        public TreeNode CreateTreeNode(string element)
        {
            TreeNode node = new TreeNode();
            node.Element = element;
            node.Left = -1;
            node.Right = -1;
            return node;
        }

        /* Inserts an element into the binary tree table */
        public bool Insert(string element)
        {
            int ID = GetNameRootID();
            if (ID == -1)  // First node so make it the root
            {
                TreeNode newNode = CreateTreeNode(element);
                db1.TreeNodes.Add(newNode);
                db1.SaveChanges();
                SetNameRootID(newNode.ID);
                return true;
            }

            // Locate the parent node
            int parentID = -1;
            string current = "";
            while (ID != -1)
            {
                int left, right;
                current = GetElement(ID, out left, out right);
                parentID = ID;
                if (element.CompareTo(current) < 0)
                    ID = left;
                else if (element.CompareTo(current) > 0)
                    ID = right;
                else    // Duplicate node not inserted
                    return false;
            }

            // Create the node to add
            TreeNode addNode = CreateTreeNode(element);
            db1.TreeNodes.Add(addNode);
            db1.SaveChanges();

            // Get the parent node
            TreeNode parentNode = GetTreeNode(parentID);
            if (parentNode == null)
                return false;

            // Attach new node to parent node
            if (element.CompareTo(current) < 0)
                parentNode.Left = addNode.ID;
            else
                parentNode.Right = addNode.ID;
            db1.SaveChanges();
            return true;
        }

        /* Deletes an element from the binary tree table */
        public bool Delete(string element)
        {
            int ID = GetNameRootID();
            int parentID = -1;
            int left = -1, right = -1;
            string current = "";

            // Loop to find node to delete and locate parent node
            while (ID != -1)
            {
                current = GetElement(ID, out left, out right);
                if (element.CompareTo(current) < 0)
                {
                    parentID = ID;
                    ID = left;
                }
                else if (element.CompareTo(current) > 0)
                {
                    parentID = ID;
                    ID = right;
                }
                else   // Found element in tree
                    break;
            }

            if (ID == -1)
                return false; // Element is not in the tree

            // Case 1: current has no left children
            if (left == -1)
            {
                // Connect parent with the right child of current
                if (parentID == -1)
                {
                    SetNameRootID(right);
                }
                else
                {
                    TreeNode parentNode = GetTreeNode(parentID);
                    if (parentNode == null)
                        return false;

                    if (element.CompareTo(parentNode.Element) < 0)
                        parentNode.Left = right;
                    else
                        parentNode.Right = right;
                    db1.SaveChanges();
                }
            }
            else
            {
                // Case 2: The current node has a left child
                // Locate the rightmost node in the left subtree of
                // the current node and also its parent
                TreeNode parentOfRightMost = GetTreeNode(ID);
                TreeNode rightMost = GetTreeNode(left);

                if (parentOfRightMost == null || rightMost == null)
                    return false;

                while (rightMost.Right != -1)
                {
                    parentOfRightMost = rightMost;
                    rightMost = GetTreeNode(rightMost.Right);
                    if (rightMost == null)
                        return false;
                }

                // Replace element in current by element in rightMost
                TreeNode currentNode = GetTreeNode(ID);
                currentNode.Element = rightMost.Element;
                db1.SaveChanges();

                // Eliminate rightmost node
                if (parentOfRightMost.Right == rightMost.ID)
                    parentOfRightMost.Right = rightMost.Left;
                else
                    // Special case: parentOfRightMost == current
                    parentOfRightMost.Left = rightMost.Left;
                db1.SaveChanges();

                ID = rightMost.ID;
            }

            TreeNode removeNode = GetTreeNode(ID);
            if (removeNode != null)
            {
                db1.TreeNodes.Remove(removeNode);
                db1.SaveChanges();
            }

            return true;
        }

        /* Returns true if the element is in the tree */
        public bool Search(string element)
        {
            int ID = GetNameRootID();
            while (ID != -1)
            {
                int left, right;
                string current = GetElement(ID, out left, out right);
                if (element.CompareTo(current) < 0)
                    ID = left;
                else if (element.CompareTo(current) > 0)
                    ID = right;
                else
                    return true;
            }
            return false;
        }

        /* InOrder traversal from the Root */
        public List<string> InOrderList()
        {
            return InOrderList(GetNameRootID(), new List<string>());
        }

        /* InOrder traversal from a subtree */
        public List<string> InOrderList(int nodeID, List<string> ls)
        {
            TreeNode node = GetTreeNode(nodeID);
            if (node == null)
                return ls;

            InOrderList(node.Left, ls);
            ls.Add(node.Element);
            InOrderList(node.Right, ls);

            return ls;
        }

        /* PreOrder traversal from the Root */
        public List<string> PreOrderList()
        {
            return PreOrderList(GetNameRootID(), new List<string>());
        }

        /* PreOrder traversal from a subtree */
        public List<string> PreOrderList(int nodeID, List<string> ls)
        {
            TreeNode node = GetTreeNode(nodeID);
            if (node == null)
                return ls;

            ls.Add(node.Element);
            PreOrderList(node.Left, ls);
            PreOrderList(node.Right, ls);

            return ls;
        }

        /* PostOrder traversal from the Root */
        public List<string> PostOrderList()
        {
            return PostOrderList(GetNameRootID(), new List<string>());
        }

        /* PostOrder traversal from a subtree */
        public List<string> PostOrderList(int nodeID, List<string> ls)
        {
            TreeNode node = GetTreeNode(nodeID);
            if (node == null)
                return ls;

            PostOrderList(node.Left, ls);
            PostOrderList(node.Right, ls);
            ls.Add(node.Element);

            return ls;
        }


        /*****************************/
        /*                           */
        /* AJAX View Display Methods */
        /*                           */
        /*****************************/

        public ActionResult GetTree()
        {
            int radius = 20; // Tree node radius
            int hGap = 200;  // Horizontal Gap
            int vGap = 50;   // Gap between two levels in a tree

            string nameTreeStr = "";
            int nameRootID = GetNameRootID();
            if (nameRootID != -1)
            {
                nameTreeStr += GetHeight(nameRootID) + ":";
                // Display tree recursively    
                nameTreeStr += DisplayTree(nameRootID,
                                          400, 30,
                                          radius, hGap, vGap);
            }
            return Content(nameTreeStr);
        }

        /* Display a subtree rooted at position (x, y) */
        private string DisplayTree(int nodeID,
                                  int x, int y, int radius,
                                         int hGap, int vGap)
        {
            int left, right;
            string element = GetElement(nodeID, out left, out right);

            // Display the root
            String treeStr = "<circle r=\"" + radius + "\"";
            treeStr += " cx=\"" + x + "\"";
            treeStr += " cy=\"" + y + "\"";
            treeStr += " stroke=\"blue\"";
            treeStr += " fill=\"none\" /> ";
            treeStr += "<text x=\"" + (x - 7) + "\"";
            treeStr += " y=\"" + (y + 4) + "\">" + element;
            treeStr += "</text>";

            if (left != -1)
            {
                // Draw a line to the left node
                treeStr += ConnectTwoCircles(x - hGap, y + vGap,
                                   x, y, vGap, radius);
                // Draw the left subtree recursively
                treeStr += DisplayTree(left, x - hGap,
                                   y + vGap, radius, hGap / 2, vGap);
            }

            if (right != -1)
            {
                // Draw a line to the right node
                treeStr += ConnectTwoCircles(x + hGap, y + vGap,
                                   x, y, vGap, radius);
                // Draw the right subtree recursively
                treeStr += DisplayTree(right, x + hGap,
                                   y + vGap, radius, hGap / 2, vGap);
            }
            return treeStr;
        }

        /* Connect two circles centered at (x1, y1) and (x2, y2) */
        public string ConnectTwoCircles(int x1, int y1, int x2, int y2,
                            double vGap, double radius)
        {
            double d = Math.Sqrt(vGap * vGap + (x2 - x1) * (x2 - x1));
            int x11 = (int)(x1 - radius * (x1 - x2) / d);
            int y11 = (int)(y1 - radius * (y1 - y2) / d);
            int x21 = (int)(x2 + radius * (x1 - x2) / d);
            int y21 = (int)(y2 + radius * (y1 - y2) / d);

            String lineStr = "<line x1=\"" + x1 + "\"";
            lineStr += " y1=\"" + y1 + "\"";
            lineStr += " x2=\"" + x2 + "\"";
            lineStr += " y2=\"" + y2 + "\"";
            lineStr += "stroke=\"cyan\" stroke-width=\"1\" /> ";

            return lineStr;
        }

        /* Get height at node given by ID */
        public int GetHeight(int ID)
        {
            if (ID == -1)
                return 0;

            int left, right;
            GetElement(ID, out left, out right);
            int leftHeight = GetHeight(left);
            int rightHeight = GetHeight(right);

            return (leftHeight > rightHeight) ?
                          leftHeight + 1 : rightHeight + 1;
        }
    }
}