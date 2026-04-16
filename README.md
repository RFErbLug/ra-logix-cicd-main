![images](5-help-documents/ra-logo.png)

# Welcome to CI/CD for Logix Control Systems!
This GitHub repository is dedicated to providing a full, comprehensive example of **Continuous Integration (CI)** and **Continuous Development (CD)** for Rockwell Automation Logix control systems. Our goal is to provide a resource for developers looking to implement CI/CD practices in their own Studio 5000 Logix Designer application development.

## Getting Started
1. Begin by referring to the [Industrial DevOps: CI/CD for Logix Control Systems Application Technique](https://literature.rockwellautomation.com/idc/groups/literature/documents/at/logix-at002_-en-p.pdf) document at `https://literature.rockwellautomation.com/idc/groups/literature/documents/at/logix-at002_-en-p.pdf` for:
- An introduction to CI/CD (What is it? Where can it be used? What benefits does it provide?)
- A list of software dependencies used by this CI/CD example system
- Instructions to verify software dependency installation

2. Once the software dependencies are installed and the Jenkins server is set up, navigate to the [workflow example](5-help-documents/WorkflowExample.txt) document for step-by-step guidance of how to navigate a Studio 5000 Logix Designer application development scenario using this example CI/CD system.

## Folder Structure
For a more detailed overview of the repository contents, navigate to the [repository contents breakdown](5-help-documents/RepositoryContentsBreakdown.txt) document.

- [1-production-files](1-production-files/) | Contains the Studio 5000 Logix Designer files to be tested and deployed.
- [2-cicd-config](2-cicd-config/) | Contains the scripts needed for full CI/CD system execution.
- [3-test-report-examples](3-test-report-examples/)  | Contains example logs of text and excel test reports, alongside example of the generated reports.
- [4-standalone-testscript](4-standalone-testscript/) | Contains a C# solution that can be run to test either a Studio 5000 Logix Designer full application or an Add-On Instruction Definition L5X. This C# solution is simpler than the '2-cicd-config/1-ci-teststage/2-ci-unittestscript/' folder C# solution because its input arguments used in the CI pipeline are removed. 
- [5-help-documents](5-help-documents/) | Contains a document that provides step-by-step guidance of how to navigate a Studio 5000 Logix Designer® application development scenario using this example CI/CD system, alongside troubleshooting suggestions & some defined script limitations. Another document further details the contents of this repository.

## Contributing
We are currently not accepting contributions at this time.

## License
Permission to modify and redistribute is granted under the terms of the MIT License.  
See the [LICENSE](LICENSE) file for the full license.

---