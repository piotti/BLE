import matplotlib.pyplot as plt

f = open('data.txt', 'r')
times = []
temps = []
setpoints = []
for l in f:
	parts = l.split(",")
	if len(parts) == 3:
		times.append(int(parts[0]) / 1000)
		temps.append(int(parts[1]))
		setpoints.append(int(parts[2].strip()))


plt.title('PID Control')

l1,=plt.plot(times, temps, 'b', label='Temperature Readings')
l2,=plt.plot(times, setpoints, 'g', label='Setpoint')
plt.grid(True)
plt.xlabel('Time (s)')
plt.ylabel('Temperature (deg C)')
plt.legend(handles=[l1, l2])


plt.show()

