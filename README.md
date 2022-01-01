# MicroSCADA
#### Video Demo:  <URL HERE>
#### Description:
Supervisory control and data acquisition (SCADA) is a system of software and hardware elements that allows industrial organizations to control and monitor various all relevant operations of a particular machinary/component or a host of machines/components that work together. 

In the engineering industry, OPC (Open Platform Communications) is the primary method by which third-party and first party OEMS and ODMS use to facilitate communication between process control via PLCs and computers. as per OPC Foundation, "The OPC standard is a series of specifications developed by industry vendors, end-users and software developers. These specifications define the interface between Clients and Servers, as well as Servers and Servers, including access to real-time data, monitoring of alarms and events, access to historical data and other applications."
  
  My primary purpose with this application was to build a small tree treversal application that can recursively scan and subcribe to nodes that contain values based on dummy tags. This practice is quite common with applications that need to use/monitor the data being trasnmitted a plant's machinary for a variety of data analysis and/or safety reasons. However, most applications in this space are quite old serving on dated technology that only works on Windows Clients. As such, MicroSCADA was a way for me to incorporate Microsoft's latest Blazoir Asp.Net Blazor Server technology using a third party API (OPC UA Cline API from Traegar) to construct a modern and client looking micro-client.
  
  I was hoping to have a live chart-view but this proved difficult with the time-constraints I had with this project, so I resorted to just a List View that gives you a view of what subcriptions to multiple tags returns in an indisutrial application.
  
Credits to:
  Microsoft
  Traeger Industry Components GmbH
