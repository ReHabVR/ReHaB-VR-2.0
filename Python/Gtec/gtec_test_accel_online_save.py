from matplotlib import pyplot as plt
import numpy as np
import statistics
import pygds

print("Inicjalizacja trochę trwa...")    
d = pygds.GDS()
print(d.Channels)
pygds.configure_demo(d) # Tu sie trzeba przyjrzec blizej - co i jak tam jest ustawiane
d.SetConfiguration()

print("Acquisition started")

samplesX = []
samplesY = []
samplesZ = []

scope = pygds.Scope(1/d.SamplingRate, title="Channels: %s", ylabel = u"U[μV]")
def processCallback(samples):
    try:
        global samplesX
        global samplesY
        global samplesZ
#        samplesX += list(samples[:,13]) # Konkatenacja :-)
#        samplesY += list(samples[:,19]) # Konkatenacja :-)
#        samplesZ += list(samples[:,31]) # Konkatenacja :-)
        strdevX = np.std(samples[:, 32], axis=0)
        # print(str(strdevX))
        if strdevX>.1:
            print("Hello world")
        samplesX += list(samples[:,32]) # Konkatenacja :-)
        samplesY += list(samples[:,33]) # Konkatenacja :-)
        samplesZ += list(samples[:,34]) # Konkatenacja :-)


        #print(len(samplesX))
    except Exception as e:
        print("ERROR")
        print(str(e))
    return scope(samples[:,32:35]) # Jak sie zamknie okno, to zwroci False i wtedy zakonczy akwizycje :-)
    
#d.GetData(d.SamplingRate//2, scope) # to standardowy use-case
print("A")
a = d.GetData(d.SamplingRate//2, processCallback) # tu uzywamy wlasnej funkcji, zeby wybrac kanały
del scope

np.savetxt("accel_data.txt", np.array([samplesX, samplesY, samplesZ]).T)
plt.plot(samplesX, label="x == left/right")
plt.plot(samplesY, label="y == up/down")
plt.plot(samplesZ, label="z == front/back")
plt.legend(loc="upper right")
plt.show()

d.Close()
del d
