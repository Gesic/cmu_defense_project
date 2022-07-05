########## How to install or set for the Project environment ##########
1. JAVA JDK(openjdk 11.0.12 2021-07-20) - https://jdk.java.net/18/
1-1. install JDK
Go to https://jdk.java.net/18/.
Select the appropriate JDK version and click Download.
1-2. setting for environment of JAVA (Set JAVA_HOME)
Right click My Computer and select Properties.
On the Advanced tab, select Environment Variables,and then edit JAVA_HOME to point to where the JDK software is located
for Example, C:\Program Files\ojdkbuild\java-1.8.0-openjdk
Check to set normally with the CMD ("java --version").
2. Eclipse(eclipse-jee-2022-06-R-win32-x86_64) -
download IDEwith eclipse for the PROJECT.
https://www.eclipse.org/downloads/packages/
select package to "Eclipse IDE for Enterprise Java and Web Developers"
3. Lombok(1.18.24) - https://projectlombok.org/download
Download java from the site(https://projectlombok.org/download)
Copy jar to Eclipse installed path..
Open the CMD(shift + mouse right click and open power shell) and run the command below.
$ java -jar lombok.jar.
Select specify location and input the eclipse path.
EX) E:\_Dev\eclipse-jee-2022-06-R-win32-x86_64\eclipse
Click the button "install / update" and quit the installer.
4. SpringFramework(2.7.0) - can be installed through the eclipse market
Execute eclipse, and select Help -> Eclipse Marketplace..
Input the text "Spring tool" or "STS" and install package(option is stay with initial setting)

########## How to import the PROJECT with eclipse##########
1. execute the IDE eclipse.
2. make "the workspace" after input the any path.
3. on the left side window, select "import project"
4. expand "Gradle" item on window, select "Existing Gradle Project"
5. find the project with "Browse.." and finish.
6. after importing project, refresh build.gradle with right click
7. Click Boot Dashboard icon on Eclipse tool(this button is green color and the "PowerOn" shape)
following tools don't have to set
* Tomcat(9.0.63) - embedded in spring boot
* update JAVA API and Libraries - Gradle Java library plugin(do refresh gradle after importing the project.)
JWT / JPA (2.7.0) / H2DB (2.1.212)

########## How to build and release (JAR)#########
1. select "RUN > Run Configuration" on the menu of Eclipse TOOL
2. Gradle Task double click and push the add button.
3. Input "bootjar" in the Gradle Task edit box.
4. Select workspace > Project(T5Defense_Server)
5. apply and run
6. check the path "T5Defense_Server\build\libs" in the project root path
