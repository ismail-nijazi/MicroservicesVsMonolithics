import subprocess
import pandas as pd
import time
from datetime import datetime
import os

NAMESPACE = "default" 
NODEGROUP_LABEL = "eks.amazonaws.com/nodegroup=ng-thesis"
NODE_CSV = "node_resource_usage.csv"
HPA_CSV = "hpa_status.csv"

# Initialize CSVs with correct headers
if not os.path.exists(NODE_CSV):
    pd.DataFrame(columns=["Date", "Node", "CPU%", "Memory%"]).to_csv(NODE_CSV, index=False)

if not os.path.exists(HPA_CSV):
    pd.DataFrame(columns=["Date", "Deployment", "CPU_Usage", "Replicas"]).to_csv(HPA_CSV, index=False)

try:
    while True:
        os.system('cls' if os.name == 'nt' else 'clear')
        timestamp = datetime.now().strftime("%H:%M:%S")

        print("=== PODS ===")
        subprocess.run(["kubectl", "get", "pods", "-n", NAMESPACE])

        print("\n=== HPA ===")
        hpa_output = subprocess.run(["kubectl", "get", "hpa"], capture_output=True, text=True)
        print(hpa_output.stdout)

        print("=== NODES RESOURCE USAGE ===")
        node_names_cmd = [
            "kubectl", "get", "nodes",
            "-l", NODEGROUP_LABEL,
            "-o", "jsonpath={range .items[*]}{.metadata.name}{\"\\n\"}{end}"
        ]
        node_names_result = subprocess.run(node_names_cmd, capture_output=True, text=True)
        node_names = node_names_result.stdout.strip().splitlines()

        node_data = []
        for node in node_names:
            top = subprocess.run(["kubectl", "top", "node", node], capture_output=True, text=True)
            lines = top.stdout.strip().splitlines()
            if len(lines) >= 2:
                values = lines[1].split()
                if len(values) >= 5: 
                    node_name = values[0]
                    cpu_percent = values[2]
                    memory_percent = values[4]
                    print(f"{node_name} - CPU%: {cpu_percent}, Memory%: {memory_percent}")
                    node_data.append({
                        "Date": timestamp,
                        "Node": node_name,
                        "CPU%": cpu_percent,
                        "Memory%": memory_percent,
                    })

        if node_data:
            pd.DataFrame(node_data).to_csv(NODE_CSV, mode="a", index=False, header=not os.path.exists(NODE_CSV))

        # === Parse HPA values ===
        hpa_lines = hpa_output.stdout.strip().splitlines()
        hpa_data = []
        for line in hpa_lines[1:]:  # skip header
            parts = line.split()
            if len(parts) >= 5:
                usage = parts[3]
                if "/" in parts[3]:
                    usage = parts[3].split("/")[0]
                hpa_data.append({
                    "Date": timestamp,
                    "Deployment": parts[0],
                    "CPU_Usage": usage.strip(),
                    "Replicas": parts[6]
                })

        if hpa_data:
            pd.DataFrame(hpa_data).to_csv(HPA_CSV, mode="a", index=False, header=False)

        time.sleep(10)

except KeyboardInterrupt:
    print("\nStopped monitoring.")
