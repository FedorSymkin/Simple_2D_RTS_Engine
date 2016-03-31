using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WOOP;

namespace IMaker
{
    public class InterfaceMaker
    {
        bool dbg;
        String dbgStr = "";
        void debug(String text)
        {
            dbgStr += text + "\n";
        }


        String output = "";

        String MethodToString(MethodInfo method, bool writePublic = true)
        {
            String prm = "";
            ParameterInfo[] prmInfo = method.GetParameters();
            foreach (var p in prmInfo) prm += String.Format("{0} {1}, ", p.ParameterType.Name, p.Name);
            if (prm.Count() > 0) prm = prm.Remove(prm.Count() - 2, 2);

            return String.Format("\t\t{0}{1} {2}({3});", 
                writePublic ? "public " : "",
                method.ReturnType.Name,
                method.Name,
                prm
                ); 
        }

        String FieldToString(FieldInfo field, bool writePublic = true)
        {
            return String.Format("\t\t{0}{1} {2};",
                writePublic ? "public " : "",
                field.FieldType.Name,
                field.Name);
        }


        String PropertyToString(PropertyInfo prop, bool writePublic = true)
        {
            String getStr = "";
            String setStr = "";

            if (prop.GetSetMethod() == null) setStr = "";
            else if (prop.GetSetMethod().IsPublic) setStr = "set;";
            else setStr = "private set;";


            if (prop.GetGetMethod() == null) getStr = "";
            else if (prop.GetGetMethod().IsPublic) getStr = "get;";
            else getStr = "private get;"; 

            return String.Format("\t\t{0}{1} {2} {{{3}{4}}}",
                writePublic ? "public " : "",
                prop.PropertyType.Name,
                prop.Name,
                getStr,
                setStr);
        }

        String MemberToString(MemberInfo member, bool writePublic = true)
        {
            String res = "";
            switch (member.MemberType)
            {
                case MemberTypes.Method: res = MethodToString((MethodInfo)member, writePublic); break;
                case MemberTypes.Field: res = FieldToString((FieldInfo)member, writePublic); break;
                case MemberTypes.Property: res = PropertyToString((PropertyInfo)member, writePublic); break;  
                default: res = "Error: this member is not supported"; break;   
            }

            string pattern = String.Format(@"\bVoid\b");
            res = Regex.Replace(res, pattern, "void");

            return res;
        }


        bool MembersCompare(MemberInfo m1, MemberInfo m2)
        {
            return (MemberToString(m1) == MemberToString(m2));
        }

        bool memberContains(MemberInfo member, MemberInfo[] containsIn)
        {
            foreach (MemberInfo m in containsIn) if (MembersCompare(member, m)) return true;
            return false;
        }

        String getInterfaceStr(String SourceCode, Type interfaceType)
        {
            String SplittedCode = SourceCode;
            int begIndex = SplittedCode.IndexOf("interface " + interfaceType.Name);
            if (begIndex == -1) begIndex = SplittedCode.IndexOf("partial interface " + interfaceType.Name);
            if (begIndex == -1) return "";
                
            SplittedCode = SplittedCode.Remove(0, begIndex);

            String res = "";
            int cntBeg = 0;
            int cntEnd = 0;
            for (int i = 0; i < SplittedCode.Count(); ++i)
            {
                if (SplittedCode[i] == '{') cntBeg++;
                if (SplittedCode[i] == '}') cntEnd++;

                res += SplittedCode[i];
                if ((cntBeg == cntEnd) && (cntBeg != 0)) break;
                
            }
            return res;
        }

        String getClassStr(String SourceCode, Type interfaceType)
        {
            String SplittedCode = SourceCode;
            int begIndex = SplittedCode.IndexOf("class " + interfaceType.Name);
            if (begIndex == -1) begIndex = SplittedCode.IndexOf("partial class " + interfaceType.Name);
            if (begIndex == -1) return "";
            SplittedCode = SplittedCode.Remove(0, begIndex);

            String res = "";
            int cntBeg = 0;
            int cntEnd = 0;
            for (int i = 0; i < SplittedCode.Count(); ++i)
            {
                if (SplittedCode[i] == '{') cntBeg++;
                if (SplittedCode[i] == '}') cntEnd++;

                res += SplittedCode[i];
                if ((cntBeg == cntEnd) && (cntBeg != 0)) break;

            }
            return res;
        }

        String addNewMembersToInterface(String oldInterfaceStr, List<MemberInfo> newMembersInClass)
        {
            if (newMembersInClass.Count == 0) return oldInterfaceStr;

            


            String res = oldInterfaceStr;
            int lindex = res.LastIndexOf("}");
            res = res.Remove(lindex, res.Count() - lindex);

            if (!oldInterfaceStr.Contains("//Automatically generated:")) res += "\r\n\t\t//Automatically generated:\r\n";
            else res = res.Remove(res.Count() - 1, 1);
            foreach (var m in newMembersInClass)
            {
                String memberStr = MemberToString(m, false);
                res += memberStr + "\r\n";

                if (dbg) debug("added: " + MemberToString(m));
            }
            res += "\t}";

            return res;
        }

        bool goodMember(MemberInfo m, Type classType, Type InterfaceType, String code)
        {
            if ((m.DeclaringType != classType) && (m.DeclaringType != InterfaceType))
            {
                if (dbg) debug(MemberToString(m) + " fails because 1");
                return false;
            }
            if (!this.memberInSource(m, code))
            {
                if (dbg) debug(MemberToString(m) + " fails because 2");
                return false;
            }

            if (m.MemberType == MemberTypes.Property) return true;
            if (m.MemberType == MemberTypes.Method)
            {
                if ((!((MethodInfo)m).IsSpecialName) == false) if (dbg) debug(MemberToString(m) + " fails because 3");

                return !((MethodInfo)m).IsSpecialName;
            }


            if (dbg) debug(MemberToString(m) + " fails because 4");
            return false;
        }

        void filterMembers(List<MemberInfo> members, Type classType, Type InterfaceType, String code)
        {
            int i = 0;
            while (i < members.Count)
            {
                if (!goodMember(members[i], classType, InterfaceType, code)) members.RemoveAt(i);
                else i++;
            }
        }

        string makeInterfaceCode(String SourceCode, Type classType, out bool wasChanges)
        {
            String localOut = "";

            wasChanges = false;
            String InterfaceName = classType.FullName;
            InterfaceName = InterfaceName.Replace(classType.Name, "I" + classType.Name);
            Type InterfaceType = Type.GetType(InterfaceName);
            if (InterfaceType == null) return SourceCode;

            localOut += ("\tPROCESSING CLASS: " + classType.FullName + "\n");

            MemberInfo[] publicMembersOfInterface = InterfaceType.GetMembers();
            MemberInfo[] publicMembersOfClass = classType.GetMembers();

            List<MemberInfo> newMembersInClass = new List<MemberInfo>();
            List<MemberInfo> deletedMembersFromInterface = new List<MemberInfo>();

            if (dbg) debug("publicMembersOfClass:");
            foreach (MemberInfo m in publicMembersOfClass)
            {
                if (dbg) debug(MemberToString(m));
                if (!memberContains(m, publicMembersOfInterface)) newMembersInClass.Add(m);
            }

            if (dbg) debug("publicMembersOfInterface:");
            foreach (MemberInfo m in publicMembersOfInterface)
            {
                if (dbg) debug(MemberToString(m));
                if (!memberContains(m, publicMembersOfClass)) deletedMembersFromInterface.Add(m);
            }




            filterMembers(newMembersInClass, classType, InterfaceType, SourceCode);
            filterMembers(deletedMembersFromInterface, classType, InterfaceType, SourceCode);

            if (dbg) debug("newMembersInClass after filter:");
            if (newMembersInClass.Count > 0)
            {
                wasChanges = true;
                localOut += (String.Format("\t\t\tNew members added to interface {0}:\n", InterfaceName));
                foreach (var m in newMembersInClass)
                {
                    localOut += ("\t\t\t\t" + MemberToString(m) + "\n");
                    if (dbg) debug(MemberToString(m));
                }
            }

            if (dbg) debug("deletedMembersFromInterface after filter:");
            if (deletedMembersFromInterface.Count > 0)
            {
                wasChanges = true;
                localOut += (String.Format("\t\t\tMembers marked as deleted in interface {0}:\n", InterfaceName));
                foreach (var m in deletedMembersFromInterface)
                {
                    localOut += ("\t\t\t\t" + MemberToString(m) + "\n");
                    if (dbg) debug(MemberToString(m));
                }
            }
                

            String OldInterfaceStr = getInterfaceStr(SourceCode, InterfaceType);
            String NewInterfaceStr = addNewMembersToInterface(OldInterfaceStr, newMembersInClass);

            String res = SourceCode;
            foreach (var m in deletedMembersFromInterface)
            {              
                string pattern = String.Format(@"\b{0}\b", m.Name);
                NewInterfaceStr = Regex.Replace(NewInterfaceStr, pattern, "<Deleted>" + m.Name);
                if (dbg) debug("deleted mark: " + m.Name);
            }

            if ((OldInterfaceStr.Length > 0) || (NewInterfaceStr.Length > 0))
            res = res.Replace(OldInterfaceStr, NewInterfaceStr);

            if (wasChanges) output += localOut;
            return res;
        }

        /*String prepareStringToCompare(String v)
        {
            String res = v;
            res = res.Replace(" ", "");
            res = res.Replace("\t", "");
            res = res.Replace("\r", "");
            res = res.Replace(";", "");
            return res;
        }*/

        bool memberInSource(MemberInfo member, String code)
        {
            String mStr = MemberToString(member);




            StringReader rd = new StringReader(code);
            while (true)
            {
                String s = rd.ReadLine();
                if (s == null) break;

                int IndexOfMember = s.IndexOf(member.Name);

               /* if (member.Name == "TTTT444")
                {
                    if (IndexOfMember >= 0)
                    {
                        int ti;
                        ti = 1;
                    }
                }*/

                string pattern = String.Format(@"\bpublic\b");
                s = Regex.Replace(s, pattern, "YIB_=7");
                int IndexOfPublic = s.IndexOf("YIB_=7");



                if ((IndexOfPublic >= 0) && (IndexOfMember >= 0))
                {
                    if (IndexOfPublic < IndexOfMember)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        List<Type> getClasses(string code)
        {
            List<Type> res = new List<Type>();
            String currentNs = "";
            StringReader rd = new StringReader(code);
            while (true)
            {
                String s = rd.ReadLine();
                if (s == null) break;
                s = s.Trim();
                s = s.Replace("partial ", "");

                if (s.IndexOf("namespace ") == 0)
                {
                    s = s.Replace("namespace ", "");
                    currentNs = s;
                }
                else if (s.IndexOf("public class ") == 0)
                {
                    s = s.Replace("public class ", "");
                    s = s.Split(':')[0].Trim();
                    Type t = Type.GetType(String.Format("{0}.{1}", currentNs, s));
                    if (t != null) res.Add(t);
                    
                }
            }

            return res;
        }


        String ProcessingCode(string code, out bool wasChanges)
        {
            if (dbg) debug("ProcessingCode");

            wasChanges = false;
            List<Type> classes = getClasses(code);
            String newCode = code;

            bool ch = false;
            foreach (var c in classes)
            {  
                newCode = makeInterfaceCode(newCode, c, out ch);
                if (ch) wasChanges = true;
            }

            return newCode;
        }

        void ProcessingFile(string filename, out bool wasChanges)
        {
            if (dbg) debug("ProcessingFile " + filename);

            String localOut = "";
            wasChanges = false;

            localOut += ("\tSAVED FILE: " + filename + "\n");
            String code = System.IO.File.ReadAllText(filename, Encoding.GetEncoding(1251));
            code = ProcessingCode(code, out wasChanges);

            if (wasChanges)
            {
                System.IO.File.WriteAllText(filename, code, Encoding.GetEncoding(1251));
                output += localOut;
            }
        }

        void Maker_Idle(object sender, EventArgs e)
        {
            dbg = W.core.getConfig("INTERFACE_MAKER_DEBUG") == "true";

            bool wasChanges = false;

            if (dbg) debug("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");

            output += ("Interface maker now working...\n");
            string[] filePaths = Directory.GetFiles(_dir, "*.cs", SearchOption.AllDirectories);
            foreach (String f in filePaths)
            {
                bool wc = false;
                ProcessingFile(f, out wc);
                if (wc) wasChanges = true;
            }

            if (wasChanges)
            {
                output += ("Interface maker finished with changes in interfaces, application will terminate\n");
                Console.WriteLine("====================================================");
                Console.WriteLine(output);
                Console.WriteLine("====================================================");
                Application.Exit();
            }
            else
            {
                Application.Idle -= Maker_Idle;
                Console.WriteLine("Interface maker finished. No changes in interfaces.");
            }

            if (dbg) File.WriteAllText("debugIntMaker.log", dbgStr);
        }

        String _dir = "NoneDir:/None";
        InterfaceMaker(String dir)
        {
            _dir = dir;
            Application.Idle += Maker_Idle;
        }

        static public void execute(string dir)
        {
            InterfaceMaker maker = new InterfaceMaker(dir);
        }
    }
    
}
