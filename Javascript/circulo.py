import matplotlib.pyplot as plt
import numpy as np


correlation_matrix = np.array([
    [1, 0.85, 0.38, 0.21, -0.79],
    [0.85, 1, -0.02, -0.02, -0.69],
    [0.38, -0.02, 1, 0.82, -0.37],
    [0.21, -0.02, 0.82, 1, -0.51],
    [-0.79, -0.69, -0.37, -0.51, 1]
])


valoresPropios = np.array([ 2.8961155318144236,
  1.6261510641067058,
  0.34007964707454186,
  0.12276711545395394,
  0.014886641550372927])


vectorPropios = np.array([
    [-0.5255635070378558, 0.271419722605394, 0.4397969798958487, 0.26664018761128583, -0.6209637357173038],
    [-0.4240393974560614, 0.508638482860011, 0.03489086071711638, -0.6793607442406718, 0.3142118555722316],
    [-0.3589374352201944, -0.5624112548944679, 0.5653238605493794, 0.05489177676978327, 0.48192667855409294],
    [-0.3537984251824001, -0.5852224515003818, -0.4005558593017832, -0.4353905855353305, -0.4270028081928131],
    [0.5384832229616553, -0.093949404269472, 0.570372843485263, -0.5242078980763615, -0.31794689341954174]
])


variables = ["Matematicas","Ciencias","Español","Historia","EdFisica"]


fig, ax = plt.subplots(figsize=(8, 8))
ax.set_xlim(-1, 1)
ax.set_ylim(-1, 1)
ax.axhline(0, color='grey', linewidth=0.5)
ax.axvline(0, color='grey', linewidth=0.5)


circle = plt.Circle((0, 0), 1, color='blue', fill=False)
ax.add_artist(circle)


for i, var in enumerate(variables):
    ax.arrow(0, 0, vectorPropios[i][0], vectorPropios[i][1], head_width=0.05, head_length=0.05, fc='red', ec='red')
    ax.text(vectorPropios[i][0]*1.15, vectorPropios[i][1]*1.15, var, color='black', ha='center', va='center')

# Mostrar el gráfico
plt.title("Gráfico de Círculo de Correlación")
plt.xlabel("Componente Principal 1")
plt.ylabel("Componente Principal 2")
plt.grid(True)
plt.show()