# Microservices vs Monolithic Architecture: Performance and Resilience Evaluation

This repository contains the source code and infrastructure configurations used in the empirical study comparing microservices and monolithic architectures, as detailed in the thesis:

**Title:** *Evaluating Performance and Resilience in Microservices and Monolithic Architectures*
**Author:** Ismail Nijazi
**Institution:** Blekinge Institute of Technology
**Date:** Maj 2025

## Overview

The project implements an Inventory Management System in both monolithic and microservices architectures using .NET Core. The applications are deployed on Amazon Elastic Kubernetes Service (EKS) to evaluate:

* **Scalability under varying traffic loads**
* **Resource utilization (CPU, memory, network)**
* **Fault tolerance and resilience mechanisms**

## Architecture

### Monolithic Version

* Single .NET Core application encapsulating all functionalities.
* Deployed as a single containerized service.

### Microservices Version

* Decomposed into multiple .NET Core services, each handling specific business capabilities.
* Services communicate via HTTP REST APIs.
* Each service is containerized and deployed independently.

## Deployment

The deployment leverages AWS EKS with the following configurations:

* `aws-cluster-setup.yaml`: Defines the EKS cluster setup.
* `cluster-autoscaler.yaml`: Configures the Cluster Autoscaler for dynamic scaling based on resource utilization.
* `InventoryManagement/k8s/`: Contains Kubernetes deployment manifests for both monolithic and microservices variants.

  * `monolith-deployment.yaml`: Deployment spec for the monolithic application.
  * `api-gateway-deployment.yaml`: Deployment for the API Gateway in the microservices architecture.
  * `product-service-deployment.yaml`: Kubernetes deployment manifest for the Product microservice.
  * `stock-service-deployment.yaml`: Kubernetes deployment manifest for the Stock microservice.
  * `services.yaml`: Exposes internal services using ClusterIP or LoadBalancer for routing within the cluster.

> **Note:** The deployment YAMLs reference publicly accessible container images hosted in the author's container registry. Users may optionally build their own images, push them to a registry of their choice (e.g., Docker Hub, ECR), and update the image fields in the manifests accordingly.

## 📊 Monitoring & Analysis

Custom Python scripts are provided to monitor and analyze system performance:

* `monitor_CPU_Memory.py`: Uses `kubectl top node` and `kubectl get hpa` commands to periodically collect CPU and memory usage for each node, as well as HPA (Horizontal Pod Autoscaler) status. The collected metrics are logged into CSV files (`node_resource_usage.csv` and `hpa_status.csv`) for further analysis.

* `monitor_network_bandwidth.py`: Retrieves the total network bandwidth (incoming and outgoing) of EKS nodes using AWS CloudWatch metrics (`NetworkIn` and `NetworkOut`) for EC2 instances in the specified node group. Data is collected for READY nodes only and logged in `nodegroup_network_bandwidth.csv`.

These tools facilitate the comparison of performance metrics between the two architectural styles under different load scenarios.

> **Note:** Both scripts depend on system command outputs (`kubectl`, `aws cli`) and assume a compatible AWS EKS and EC2 setup. While they functioned correctly during the time of this study, future users should validate their correctness in newer environments, as command output formats or APIs may change over time.

## Repository Structure

```
MicroservicesVsMonolithics/
├── InventoryManagement/           # Application source code
│   ├── Monolithic/                # Monolithic architecture implementation
│   ├── Microservices/             # Microservices architecture implementation
│   └── k8s/                       # Kubernetes deployment files for both architectures
├── aws-cluster-setup.yaml         # AWS EKS cluster configuration
├── cluster-autoscaler.yaml        # Cluster Autoscaler configuration
├── monitor_CPU_Memory.py          # CPU and memory monitoring script
├── monitor_network_bandwidth.py   # Network bandwidth monitoring script
└── README.md                      # Project documentation
```

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/ismail-nijazi/MicroservicesVsMonolithics/blob/main/LICENSE) file for details.
