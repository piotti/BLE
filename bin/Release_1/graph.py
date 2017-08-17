import matplotlib.pyplot as plt

f = open('data.txt', 'r')
times = []
temps = []
setpoints = []
outs = []
for l in f:
	parts = l.split(",")
	if len(parts) == 4:
		times.append(int(parts[0]) / 1000)
		temps.append(float(parts[1]))
		setpoints.append(int(parts[2]))
		outs.append(int(parts[3].strip()))

outs = [o/10 for o in outs]



plt.subplot(2,1,1)
plt.title('PID Control')
l1,=plt.plot(times, temps, 'b', label='Temperature Readings')
l2,=plt.plot(times, setpoints, 'g', label='Setpoint')
plt.grid(True)

plt.ylabel('Temperature (deg C)')
plt.legend(handles=[l1, l2])


plt.subplot(2,1,2)
plt.plot(times, outs)
plt.grid(True)
plt.ylabel('Output (% Duty Cycle)')



plt.xlabel('Time (s)')


plt.show()

