from matplotlib import pyplot as plt

import pygds

print("Inicjalizacja trochę trwa...") 
d = pygds.GDS()
pygds.configure_demo(d) # Tu sie trzeba przyjrzec blizej - co i jak tam jest ustawiane
d.SetConfiguration()

print("Acquisition started (4sec)")
a=d.GetData(d.SamplingRate * 4) # pobieramy 4 sekundy danych
print(a.shape)
plt.plot(a[:,13], label="C5 right hand")
plt.plot(a[:,21], label="C6 left hand")
plt.plot(a[:,31], label="POZ")
# plt.plot(a[:,32], label="x == left/right")
# plt.plot(a[:,33], label="y == up/down")
# plt.plot(a[:,34], label="z == front/back")
plt.legend(loc="upper right")
plt.show()

d.Close()
del d
