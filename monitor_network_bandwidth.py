import subprocess
import json
import csv
import os
from datetime import datetime, timedelta
import time

# === CONFIG ===
NODEGROUP_NAME = "ng-thesis"
REGION = "eu-north-1"
CSV_FILE = "nodegroup_network_bandwidth.csv"

# === Initialize CSV file if it doesn't exist ===
if not os.path.exists(CSV_FILE):
    with open(CSV_FILE, mode="w", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(["Time", "NodeCount", "NetworkBandwidth(KB/s)"])

# === Step 1: Get Instance IDs in the Node Group ===
def get_instance_ids():
    cmd = [
        "aws", "ec2", "describe-instances",
        "--region", REGION,
        "--filters",
        f"Name=tag:eks:nodegroup-name,Values={NODEGROUP_NAME}",
        "Name=instance-state-name,Values=running",
        "--query", "Reservations[*].Instances[*].InstanceId",
        "--output", "json"
    ]
    raw = subprocess.check_output(cmd)
    ids = json.loads(raw)
    return [iid for sublist in ids for iid in sublist]

# === Step 1.5: Filter to only nodes that are Ready in Kubernetes ===
def filter_ready_nodes(instance_ids):
    try:
        kubectl_output = subprocess.check_output(["kubectl", "get", "nodes", "-o", "json"], text=True)
        node_info = json.loads(kubectl_output)
        
        ready_instance_ids = []

        for node in node_info["items"]:
            # Check Ready condition
            conditions = node["status"]["conditions"]
            is_ready = any(cond["type"] == "Ready" and cond["status"] == "True" for cond in conditions)

            if is_ready:
                # Match the EC2 instance ID from the node's providerID
                provider_id = node["spec"].get("providerID", "")
                if "aws://" in provider_id:
                    instance_id = provider_id.split("/")[-1]
                    if instance_id in instance_ids:
                        ready_instance_ids.append(instance_id)

        return ready_instance_ids
    except Exception as e:
        print(f"⚠️ Error checking node readiness: {e}")
        return []

# === Step 2: Get Network Metrics from CloudWatch ===
def get_network_usage(instance_id, metric_name):
    now = datetime.utcnow()
    end_time = now.strftime("%Y-%m-%dT%H:%M:%SZ")
    start_time = (now - timedelta(minutes=1)).strftime("%Y-%m-%dT%H:%M:%SZ")

    cmd = [
        "aws", "cloudwatch", "get-metric-statistics",
        "--namespace", "AWS/EC2",
        "--metric-name", metric_name,
        "--dimensions", f"Name=InstanceId,Value={instance_id}",
        "--start-time", start_time,
        "--end-time", end_time,
        "--period", "60",
        "--statistics", "Sum",
        "--unit", "Bytes",
        "--region", REGION,
        "--output", "json"
    ]
    result = subprocess.check_output(cmd)
    data = json.loads(result)
    if data["Datapoints"]:
        return data["Datapoints"][0]["Sum"]  # in bytes
    return 0

# === Step 3: Calculate total network and write to CSV ===
def log_bandwidth():
    all_instance_ids = get_instance_ids()
    instance_ids = filter_ready_nodes(all_instance_ids)

    if not instance_ids:
        print("❌ No READY instances found.")
        return

    total_in = 0
    total_out = 0

    for iid in instance_ids:
        total_in += get_network_usage(iid, "NetworkIn")
        total_out += get_network_usage(iid, "NetworkOut")

    total_bandwidth_kb = (total_in + total_out) / 1024  # in KB/s
    timestamp = datetime.utcnow().strftime("%H:%M:%S")
    node_count = len(instance_ids)

    print(f"[{timestamp}] Ready Nodes: {node_count} | Bandwidth: {total_bandwidth_kb:.2f} KB/s")

    with open(CSV_FILE, mode="a", newline="") as f:
        writer = csv.writer(f)
        writer.writerow([timestamp, node_count, f"{total_bandwidth_kb:.2f}"])

# === Run repeatedly ===
while True:
    log_bandwidth()
    time.sleep(60)
