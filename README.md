[![Build
status](https://ci.appveyor.com/api/projects/status/ox0qebg3wv3dp30e/branch/develop?svg=true)](https://ci.appveyor.com/project/KrzysztofPajak/grandnode/branch/develop) [![Build Status](https://travis-ci.org/grandnode/grandnode.svg?branch=develop)](https://travis-ci.org/grandnode/grandnode)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/acbd143050984c1983d7cb0bd10b3472)](https://www.codacy.com/app/grandnode/grandnode?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=grandnode/grandnode&amp;utm_campaign=Badge_Grade)
![Github All Releases](https://img.shields.io/github/downloads/grandnode/grandnode/total.svg) [![Demo](https://img.shields.io/badge/DEMO-HERE-green.svg)](https://demo.grandnode.com/)

<br />
Dear Community, 5 years ago, GrandNode was created on the basis of nopCommerce. We are therefore obligated to inherit their license. This license does not allow us to use 100% of the benefits offered by open-source, it inhibits the development of our application. We are working on a completely new platform that will allow you to use 100% of the potential of .NET Core 5.0 and MongoDB. In the next few days we will share an adress of new repository, where the develop version of the incoming project will be published. Stay tuned! Please note that bug fixing for the current version of GrandNode, will be supported until the end of 2022.
<br /><br />
<p align="center">
  <a href="https://grandnode.com/">
    <img src="https://grandnode.com/content/images/uploaded/Blog/githubnew1.jpg" alt="Logo">
  </a>

  <h1 align="center">GrandNode</h1>

  <p align="center">
    Cloud friendly, All-in-One, Open-Source, Free e-Commerce Platform 
    <br />
    <a href="https://grandnode.com/?utm_source=github&utm_medium=link&utm_campaign=readme"><strong>Explore the project »</strong></a>
    <br />
    <br />
    <a href="https://demo.grandnode.com/?utm_source=github&utm_medium=link&utm_campaign=readme">View Demo</a>
    ·
    <a href="https://github.com/grandnode/grandnode/issues">Report Bug</a>
    ·
    <a href="https://github.com/grandnode/grandnode/issues">Request Feature</a>
    ·
    <a href="https://grandnode.com/boards/?utm_source=github&utm_medium=link&utm_campaign=readme">Visit forum</a>
    ·
    <a href="https://grandnode.com/grandnode-themes/?utm_source=github&utm_medium=link&utm_campaign=readme">Premium Themes</a>
    ·
    <a href="https://grandnode.com/extensions/?utm_source=github&utm_medium=link&utm_campaign=readme">Integrations & Plugins</a>
    ·
    <a href="https://grandnode.com/premium-support-packages/?utm_source=github&utm_medium=link&utm_campaign=readme">Premium support</a>
  </p>
</p>



<!-- TABLE OF CONTENTS -->
## Table of Contents

* [Why GrandNode?](#about-the-project)
  * [Technology Stack](#built-with)
* [Getting Started](#getting-started)
  * [Prerequisites](#prerequisites)
  * [Installation](#installation)
  * [Online demo](#online-demo)
* [Awesome projects](#Awesome-projects)
* [Roadmap](#roadmap)
* [Contributing](#contributing)
* [License](#license)



## About The Project

![GithHub Header](https://grandnode.com/content/images/uploaded/Blog/gitbanner.jpg)

GrandNode is an e-commerce platform for developing online stores. It gives you possibility to create highly advanced, good-looking online stores which have unlimited power of customization. 

### The store owner challenges

GrandNode was designed to solve the most important business challenges from the world of digital shopping. The goal for us is to provide the platform with:
* The high performance front-end, rendered within miliseconds,
* The high performance application to handle temporary and permanent traffic overloads,
* Highly advanced e-commerce platform with unlimited possibilities of integration with existing third-party softwares
* Fast development with modern codebase
* Scalable e-commerce platform to grow with the business


### Built With

![Technology Stack](https://grandnode.com/content/images/uploaded/Blog/technologystack1.JPG)


<!-- GETTING STARTED -->
## Getting Started

To get a local copy up and running follow these simple steps.

### Prerequisites

GrandNode requires .NET Core 3.1, MongoDB 4.0+, and OS-specific dependency tools. 

### Installation

GrandNode can be installed in a few different ways. Note: The develop branch is the development version of GrandNode and it may be unstable. To use the
latest stable version, download it from the Releases page or switch to a release branch. 

* Docker 
```
docker run -d -p 127.0.0.1:27017:27017 --name mongodb mongo 
docker run -d -p 80:80 --name grandnode --link mongodb:mongo grandnode/develop
``` 
If you want to download the latest stable version of GrandNode please use the following command, where x.xx is a number of GrandNode release: 
```
docker pull grandnode/grandnode:x.xx 
```

* Open locally with VS2019+

Run the project in the Visual Studio 2019+, extract the source code package downloaded from Releases tab to a folder. Enter the extracted folder and double-click the GrandNode.sln solution file. Select the Plugins project, rebuild it, then select the GrandNode.Web project.

* Host on Linux server 

Before you start - please install, configure the nginx server, .NET Core 3.1+ and MongoDB 4.0+
```
mkdir ~/source
cd ~/source
git clone - b x.xx https://github.com/grandnode/grandnode.git
```
```
cd ~/source/grandnode
dotnet restore GrandNode.sln
```
Now it's time to rebuild all of our plugins and publish application (command is pretty long because we've combined all commands in a single line, to ease up your work):
```
sudo dotnet build Plugins/Grand.Plugin.DiscountRequirements.Standard && sudo dotnet build Plugins/Grand.Plugin.ExchangeRate.McExchange && sudo dotnet build Plugins/Grand.Plugin.ExternalAuth.Facebook && sudo dotnet build Plugins/Grand.Plugin.Payments.CashOnDelivery && sudo dotnet build Plugins/Grand.Plugin.Payments.BrainTree && sudo dotnet build Plugins/Grand.Plugin.ExternalAuth.Google && sudo dotnet build Plugins/Grand.Plugin.Payments.PayPalStandard && sudo dotnet build Plugins/Grand.Plugin.Shipping.ByWeight && sudo dotnet build Plugins/Grand.Plugin.Shipping.FixedRateShipping && sudo dotnet build Plugins/Grand.Plugin.Shipping.ShippingPoint && sudo dotnet build Plugins/Grand.Plugin.Tax.CountryStateZip && sudo dotnet build Plugins/Grand.Plugin.Tax.FixedRate && sudo dotnet build Plugins/Grand.Plugin.Widgets.FacebookPixel && sudo dotnet build Plugins/Grand.Plugin.Widgets.GoogleAnalytics && sudo dotnet build Plugins/Grand.Plugin.Widgets.Slider && sudo dotnet publish Grand.Web -c Release -o /var/webapps/grandnode
```
Optional: Create the service file, to automatically restart your application.
```
sudo vi /etc/systemd/system/grandnode.service
```
Paste the following content, and save changes:
```
[Unit]
Description=GrandNode

[Service]
WorkingDirectory=/var/webapps/grandnode
ExecStart=/usr/bin/dotnet /var/webapps/grandnode/Grand.Web.dll
Restart=always
RestartSec=10
SyslogIdentifier=dotnet-grandnode
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```
Enable the service and restart the GrandNode
```
sudo systemctl enable grandnode.service
sudo systemctl start grandnode.service
``` 
Feel free to visit our [detailed guide about GrandNode installation.](https://grandnode.com/how-to-install-grandnode-on-linux-ubuntu-1604/?utm_source=github&utm_medium=link&utm_campaign=readme)

Install GrandNode with one click on [DigitalOcean](https://marketplace.digitalocean.com/apps/grandnode)


### Online demo 
#### Frontend #### 
[https://demo.grandnode.com/](https://demo.grandnode.com/?utm_source=github&utm_medium=link&utm_campaign=readme)

#### Backend #### 
[https://demo.grandnode.com/admin](https://demo.grandnode.com/admin/?utm_source=github&utm_medium=link&utm_campaign=readme) 


Demo is restoring once per day to the original state. 

Access to the admin panel:

Admin email: admin@yourstore.com 

Admin password: 123456


## Awesome projects

[![Awesome projects](https://grandnode.com/content/images/uploaded/Blog/awesomeprojectsgit1.JPG)](https://grandnode.com/showcase/?utm_source=github&utm_medium=link&utm_campaign=readme)

Check the [GrandNode Live Projects](https://grandnode.com/showcase/?utm_source=github&utm_medium=link&utm_campaign=readme).

Have you done something great with GrandNode? Let us know and get listed!


## Roadmap

We have a clear vision in which direction we would like to develop GrandNode. Ready roadmaps with milestones for future versions of GrandNode can be found in the [projects tab](https://github.com/grandnode/grandnode/projects).

## Contributing

GrandNode is and always will be free and open-source.

You can support the project in many several ways:
- Contribute
- Evangelize - Maybe drop some blog post, tweet, or LinkedIn publication? Mention us with #GrandNode. 
- Become a partner - We have a special solution partner program for developers and agencies. Join us and you will receive all our paid themes and 10 premium plugins of your choice. [Check how to become a GrandNode partner.](https://grandnode.com/partnershipprogram/?utm_source=github&utm_medium=link&utm_campaign=readme)

:star: Star us on GitHub - it's the first step to become a GrandNode supporter! GrandNode is an open source online shopping solution, each developer is welcome and encouraged to contribute with their own improvements and enhancements.

GrandNode is mostly written in ASP.NET. Other languages used in the project are HTML, CSS, JavaScript, MongoDB. To start with us, you should do this few steps: 

1. Create your own GitHub account. 

2. Fork the GrandNode to your GitHub account

3. Clone the forked project to your local machine

4. After that, create a branch for your own changes

5. Change the files. 6. Push your changes from local machine to your fork in your GitHub account 

7. It's time to create a pull request for your changes on the GrandNode project. If you don't know how to do it, you can read more about pull request [here](https://help.github.com/articles/about-pull-requests/) 

8. Wait for the information. One of our developers will comment your changes and approve it or will suggest some
improvements in your code. 

And that's all, you are GrandNode official contributor! [Coding standards and guides](https://docs.grandnode.com/developer-guides)


## License

Distributed under the GNU General Public License v3.0. It's available [here](https://github.com/grandnode/grandnode/blob/develop/LICENSE)
