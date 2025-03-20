from matplotlib import pyplot as plt

import pygds
    
print("Inicjalizacja trochę trwa...")
d = pygds.GDS()
pygds.configure_demo(d) # Tu sie trzeba przyjrzec blizej - co i jak tam jest ustawiane
d.SetConfiguration()

print("Acquisition started")

scope = pygds.Scope(1/d.SamplingRate, title="Channels: %s", ylabel = u"U[μV]")
def processCallback(samples):
    print(len(samples[0]))
    return scope(samples[:,32:35]) # Jak sie zamknie okno, to zwroci False i wtedy zakonczy akwizycje :-)
    
#d.GetData(d.SamplingRate//2, scope) # to standardowy use-case
d.GetData(d.SamplingRate//7, processCallback) # tu uzywamy wlasnej funkcji, zeby wybrac kanały
del scope

d.Close()
del d
