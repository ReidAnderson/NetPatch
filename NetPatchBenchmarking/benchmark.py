import subprocess
import csv
import json
import datetime

PIPE = subprocess.PIPE
process = subprocess.Popen(['./gitInfo.sh'], stdout=PIPE, stderr=PIPE)
stdoutput, stderroutput = process.communicate()

print(str(stdoutput))

subprocess.call(['dotnet', 'run', '--configuration', 'Release'])


date_time = datetime.datetime.utcnow().strftime("%Y-%m-%dT%H:%M:%SZ")

new_rows = []
with open('BenchmarkDotNet.Artifacts/results/NetPatchBenchmarking.Test-report.csv') as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=',')
    first_row = True

    for row in csv_reader:
        if not first_row:
            new_rows.append([stdoutput.decode("utf-8").strip(), date_time] + row)
        else:
            first_row = False

with open('benchmarkResults.csv', mode='a') as csv_file:
    record_writer = csv.writer(csv_file, delimiter=',', quotechar='"')

    for row in new_rows:
        record_writer.writerow(row)
    
print(json.dumps(new_rows))

