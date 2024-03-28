
using System.Collections;
using System.Globalization;
using Accord.Math;
using Accord.Statistics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using Accord.Math.Decompositions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using SkiaSharp;

namespace ACP_MiniProyecto;
class Program
{

    static string[]? Subjects;
    static string[]? Students;
    static void Main(string[] args)
    {
        string file = "EjemploEstudiantes.csv";
        double[,]? matrix;

        if (!File.Exists(file))
        {
            Console.WriteLine($"The file '{file}' does not exist.");
            return;
        }

        matrix = Open_csv(file);

        if (matrix == null || Subjects == null || Students == null)
        {
            return;
        }

        Console.WriteLine("SUBJECTS");
        PrintArray(Subjects);
        Console.WriteLine("STUDENTS");
        PrintArray(Students);


        double[,] normalizedMatrix = NormalizedMatrix(matrix);

        double[,] correlationMatrix = CorrelationMatrix(normalizedMatrix);

        double[] eigenValues = Eigenvalues(correlationMatrix);

        double[,] eigenVectors = Eigenvectors(correlationMatrix);

        double[,] pcMatrix = PrincipalComponentMatrix(normalizedMatrix, eigenVectors);
        Console.WriteLine("Principal Component Matrix:");
        PrintMatrix(pcMatrix);

        double[,] iqMatrix = IndividualQualitiesMatrix(normalizedMatrix, normalizedMatrix);
        Console.WriteLine("Individual Qualities Matrix:");
        PrintMatrix(iqMatrix);

        double[,] ZScoresMatrix = ZScoreMatrix(normalizedMatrix);
        Console.WriteLine("Coordinate Matrix (Z-Scores)");
        PrintMatrix(ZScoresMatrix);

        double[,] qualityMatrix = QualityMatrix(normalizedMatrix, correlationMatrix);
        Console.WriteLine("Quality Matrix");
        PrintMatrix(qualityMatrix);

        Vector<double> vectorInercia = AxisInertiaVector(eigenValues, normalizedMatrix.GetLength(1));
        Console.WriteLine("Vector Inercia de Ejes");
        Console.WriteLine(vectorInercia);

        Point[,] circleGraphPoints = GeneratePoints(ZScoresMatrix, true);
        Point[,] graphPoints = GeneratePoints(ZScoresMatrix, false);

        // Console.WriteLine("CIRCLE: ");
        // PrintMatrix(circleGraphPoints);
        // Console.WriteLine("SQUARE: ");
        // PrintMatrix(graphPoints);

        for (int i = 0; i < Matrix.Rows(circleGraphPoints); i++)
        {
            string[]? labels = null;

            for (int j = 0; j < Students.Length; j += 2)
            {
                labels = SeparateLabels(Students, j);

                if (labels != null)
                {
                    break;
                }
            }

            if (labels != null)
            {
                Point[]? circleGraphDrawPoints = SeparatePoints(circleGraphPoints, i);

                if (circleGraphDrawPoints != null)
                {
                    GenerateImage(1080, 1080, "circle_graph" + "_" + i + ".png",
                        circleGraphDrawPoints, labels[0], labels[1], true);
                }
            }
        }

        for (int i = 0; i < Matrix.Rows(graphPoints); i++)
        {
            string[]? labels = null;

            for (int j = 0; j < Subjects.Length; j += 2)
            {
                labels = SeparateLabels(Subjects, j);

                if (labels != null)
                {
                    break;
                }
            }

            if (labels != null)
            {
                Point[]? graphDrawPoints = SeparatePoints(graphPoints, i);

                if (graphDrawPoints != null)
                {
                    GenerateImage(1080, 1080, "graph" + "_" + i + ".png", graphDrawPoints, labels[0], labels[1], false);
                }
            }
        }
        Console.WriteLine("Las graficas estan ubicadas en el directorio:");
        Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory+"\n");

        Console.WriteLine("Pulse Enter para salir...");
        Console.ReadLine();
    }

    static string[]? SeparateLabels(string[] labels, int index)
    {
        return [labels[index], labels[index + 1]];
    }

    static Point[]? SeparatePoints(Point[,] points, int row)
    {
        int cols = Matrix.Columns(points);

        Point[] pointsArray = new Point[cols];

        for (int i = 0; i < cols; i++)
        {
            pointsArray[i] = points[row, i];
        }

        return pointsArray;
    }

    /// <summary>
    /// Generates the points to draw in the graph given the correlation matrix.
    /// </summary>
    static Point[,] GeneratePoints(double[,] coordinateMatrix, bool circleGraph)
    {
        if (circleGraph)
        {
            return GeneratePointsForCircleGraph(coordinateMatrix);
        }
        else
        {
            return GeneratePointsForSquareGraph(coordinateMatrix);
        }
    }

    /// <summary>
    /// Funcion que genera los puntos utilizados para la grafica del plano principal.
    /// </summary>
    static Point[,] GeneratePointsForSquareGraph(double[,] coordinateMatrix)
    {
        int columns = coordinateMatrix.GetLength(1);
        int rows = coordinateMatrix.GetLength(0);
        int numPoints = columns / 2;

        Point[,] points = new Point[numPoints, rows];

        for (int i = 0; i < numPoints; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                double x = coordinateMatrix[j, i * 2];
                double y = coordinateMatrix[j, i * 2 + 1];
                Point point = new(x, y);

                if (Students != null)
                {
                    point.PointName = Students[j];
                }

                points[i, j] = point;
            }
        }

        return points;
    }

    /// <summary>
    /// Funcion que genera los puntos utilizados para la grafica del circulo.
    /// </summary>
    static Point[,] GeneratePointsForCircleGraph(double[,] coordinateMatrix)
    {
        coordinateMatrix = TransposeMatrix(coordinateMatrix);

        int columns = coordinateMatrix.GetLength(1);
        int rows = coordinateMatrix.GetLength(0);
        int numPoints = columns / 2;

        Point[,] points = new Point[numPoints, rows];

        for (int i = 0; i < numPoints; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                double x = coordinateMatrix[j, i * 2];
                double y = coordinateMatrix[j, i * 2 + 1];
                Point point = new(x, y);
                if (Subjects != null)
                {
                    point.PointName = Subjects[j];
                }
                points[i, j] = point;
            }
        }

        return points;
    }

    /// <summary>
    /// Funcion que retorna la transpuesta de una matriz
    /// </summary>

    static double[,] TransposeMatrix(double[,] matrix)
    {
        return Matrix.Transpose(matrix);
    }

    /// <summary>
    /// DEPRICATED: funcion que genera puntos aleatorios en un rango dado
    /// </summary>

    static double[,] GenerateRandomPoints()
    {
        Random rnd = new();

        int numPoints = rnd.Next(20, 31);

        double[,] points = new double[numPoints, 2];

        for (int i = 0; i < numPoints; i++)
        {
            double x = -1 + 2 * rnd.NextDouble();
            double y = -1 + 2 * rnd.NextDouble();

            points[i, 0] = x;
            points[i, 1] = y;
        }

        return points;
    }

    /// <summary>
    /// Opens the generated image automatically once rendered.
    /// </summary>
    static void OpenGeneratedImage(string filename)
    {
        ProcessStartInfo psi = new()
        {
            FileName = filename,
            UseShellExecute = true
        };
        Process.Start(psi);
    }

    /// <summary>
    /// Generates an image with the GraphGenerator class
    /// and saves it as a PNG in the specified <paramref name="filename"/>
    /// </summary>
    static void GenerateImage(int width, int height, string filename, Point[] points, string xLabel, string yLabel, bool circleGraph = true)
    {
        if (Subjects == null || Students == null)
        {
            Console.WriteLine("Headers or RowHeaders are NULL!");
            return;
        }

        GraphGenerator graphGenerator = new()
        {
            Points = points,
            RowHeaders = Students,
            XAxisLabel = xLabel,
            YAxisLabel = yLabel,
        };

        SKBitmap bitmap = graphGenerator.DrawCartesianPlane(width, height, circleGraph);

        // if(circleGraph)
        // {
        //     bitmap = GraphGenerator.DrawCartesianPlane(width, height);
        // }
        // else 
        // {
        //     bitmap = GraphGenerator.DrawCartesianPlane(width, height, false);
        // }

        using FileStream stream = File.OpenWrite(filename);
        bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);

        // OpenGeneratedImage(filename);
    }

    /// <summary>
    /// Pretty prints a two-dimensional array of doubles.
    /// </summary>
    static void PrintMatrix(double[,] matrix)
    {
        Console.WriteLine(Matrix<double>.Build.DenseOfArray(matrix));
    }

    /// <summary>
    /// Impresion tradicional de un arreglo
    /// </summary>

    static void PrintArray(string[] array)
    {
        Console.Write("[ ");
        for (int i = 0; i < array.Length; i++)
        {
            Console.Write(array[i] + ", ");
        }
        Console.WriteLine(" ]");
    }

    /// <summary>
    /// Pretty prints a two-dimensional array of Points.
    /// </summary>
    public static void PrintMatrix(Point[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Console.Write($"({matrix[i, j].X:0.####}, {matrix[i, j].Y:0.####})".PadLeft(20));
            }
            Console.WriteLine();
        }
    }

    public static void PrintMatrix(Point[] matrix)
    {
        if (matrix == null)
        {
            Console.WriteLine("Matrix is null");
            return;
        }

        int length = matrix.Length;
        Console.WriteLine(length);

        for (int i = 0; i < length; i++)
        {
            Console.Write($"({matrix[i].X:0.####}, {matrix[i].Y:0.####})".PadLeft(20));
        }
    }


    /// <summary>
    /// Pretty prints a one-dimensional array of doubles.
    /// </summary>
    static void PrintMatrix(double[] matrix)
    {
        Console.WriteLine(Vector<double>.Build.DenseOfArray(matrix));
    }

    /// <summary>
    /// Funcion que lee el archivo pasando el nombre como parametro.
    /// Retornara una matriz de doubles.
    /// </summary>
    static double[,]? Open_csv(string fileName)
    {
        ArrayList? read_list = [];

        try
        {
            //Apertura del CSV, lectura y guardado de lineas
            using StreamReader reader = new(fileName);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                read_list.Add(line);
            }
        }
        catch (Exception)
        {
            throw;
        }

        //Definir el formato dado en coma decimal
        CultureInfo culture = new("fr-FR");

        //Eliminar los encabezados de la tabla dada
        string var = (string)read_list[0];
        Subjects = (var.Trim()).Split(';');
        read_list.RemoveAt(0);

        //Contador que mueve las filas a llenar
        int counter = 0;
        string[]? temp = [];

        //Arreglo inicial de datos para conocer la cantidad de columnas
        if (read_list.Count > 0 && read_list[0] is string v)
        {
            temp = v.Split(';');
        }

        //Instancia de la nueva matriz de tipo Double
        if (temp == null)
        {
            return null;
        }

        //Variables
        double[,]? matrix = new double[read_list.Count, temp.Length - 1];
        Students = new string[read_list.Count];

        //Se recorre todo el ArrayList para insertar los valores en la matriz
        int flag = 0;
        for (int k = 0; k < read_list.Count; k++)
        {
            string[]? line = [];

            //divide los numeros separados por ;
            if (read_list[k] is string v2)
            {
                line = v2.Split(';');
            }

            if (line != null)
            {
                for (int i = 0; i < line.Length; i++)
                {

                    if (i == 0)
                    {
                        Students[flag] = line[i];
                        flag++;
                    }
                    else
                    {
                        matrix[counter, i - 1] = double.Parse(line[i], NumberStyles.Float, culture);
                    }


                }
            }
            //llena la fila en counter con los numeros asignando el casteo
            counter++;
        }

        return matrix;
    }

    /// <summary>
    /// Returns the normalized form of a two-dimensional array of doubles
    /// </summary>
    static double[,] NormalizedMatrix(double[,] matrix)
    {
        int rows = Matrix.Rows(matrix);
        int cols = Matrix.Columns(matrix);

        double[] normalizedVector = Matrix.Reshape(matrix);
        Matrix.Normalize(normalizedVector, true);
        double[,] normalizedMatrix = Matrix.Reshape(normalizedVector, rows, cols);

        return normalizedMatrix;
    }

    /// <summary>
    /// Returns the correlation matrix of a two-dimensional array of doubles
    /// </summary>
    static double[,] CorrelationMatrix(double[,] matrix)
    {
        return Measures.Correlation(matrix);
    }

    /// <summary>
    /// Returns the eigenvalues (proper values) of a two-dimensional array of doubles
    /// </summary>
    static double[] Eigenvalues(double[,] matrix)
    {
        EigenvalueDecomposition eigenValuesClass = new(matrix, false, true);
        double[] eigenValues = eigenValuesClass.RealEigenvalues;
        return eigenValues;
    }

    /// <summary>
    /// Returns the eigenvectors (proper vectors) of a two-dimensional array of doubles
    /// in the form of a two-dimensional matrix.
    /// </summary>
    static double[,] Eigenvectors(double[,] matrix)
    {
        EigenvalueDecomposition eigenValuesClass = new(matrix, false, true);
        double[,] eigenVectors = eigenValuesClass.Eigenvectors;

        // TODO:
        // Por favor revisar si este método de ordenar la matriz es correcto,
        // Si no, existen otras maneras con el Sort.
        // if(!Matrix.IsSorted(Matrix.Reshape(eigenVectors)))
        // {
        //     Matrix.Sort(Eigenvalues(matrix), eigenVectors);
        // }

        return eigenVectors;
    }

    /// <summary>
    /// Returns the principal component matrix, which is the dot product of the normalized
    /// matrix and its eigenvectors matrix.
    /// </summary>
    static double[,] PrincipalComponentMatrix(double[,] matrix, double[,] eigenVectors)
    {
        return Matrix.Dot(matrix, eigenVectors);
    }

    /// <summary>
    /// Returns the principal component matrix, which is an algorithm given by
    /// the principal component matrix and the normalized matrix.
    /// </summary>
    static double[,] IndividualQualitiesMatrix(double[,] pcMatrix, double[,] matrix)
    {

        int n = Matrix.Rows(pcMatrix);
        int m = Matrix.Columns(pcMatrix);

        double[,] iqMatrix = new double[n, m];

        for (int i = 0; i < n; i++)
        {
            for (int r = 0; r < m; r++)
            {
                double numerator = Math.Pow(pcMatrix[i, r], 2);
                double denominator = 0;

                for (int j = 0; j < m; j++)
                {
                    denominator += Math.Pow(matrix[i, j], 2);
                }

                iqMatrix[i, r] = numerator / denominator;
            }
        }

        return iqMatrix;
    }

    /// <summary>
    /// Funcion que calculara la matriz de coordenadas 
    /// de una matriz previamente normalizada.
    /// </summary>
    static double[,] ZScoreMatrix(double[,] normalizedMatrix)
    {

        // --------------------------------------------------------
        // @Daniel, por favor revisar:
        // https://www.statology.org/z-score-normalization/
        // --------------------------------------------------------

        // int rows = normalizedMatrix.GetLength(0);
        // int cols = normalizedMatrix.GetLength(1);
        // double[,] coordinateMatrix = new double[rows, cols];

        // for (int i = 0; i < cols; i++)
        // {

        //     double[] columna = new double[rows];

        //     for (int j = 0; j < rows; j++)
        //     {

        //         columna[j] = normalizedMatrix[j, i];
        //     }

        //     //calcular la media y el stdDev
        //     double media = columna.Mean();
        //     double dev = columna.StandardDeviation();

        //     //Calculo de Z-Scores
        //     for (int k = 0; k < rows; k++)
        //     {
        //         coordinateMatrix[k, i] = (normalizedMatrix[k, i] - media) / dev;
        //     }
        // }

        double[,] coordinateMatrix = Accord.Statistics.Tools.ZScores(normalizedMatrix);

        return coordinateMatrix;
    }

    /// <summary>
    /// Funcion que retorna la matriz de calidades usando
    /// la matriz de correlaciones.
    /// </summary>
    static double[,] QualityMatrix(double[,] normalizedMatrix, double[,] correlationMatrix)
    {

        int cols = normalizedMatrix.GetLength(1);
        double[,] qualityMatrix = new double[cols, cols];

        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                qualityMatrix[i, j] = Math.Abs(correlationMatrix[i, j]);
            }
        }

        return qualityMatrix;
    }

    /// <summary>
    /// Funcion que retorna un Vector con el calculo de vector de inercia
    /// a partir del uso de los valores propios. 
    /// </summary>
    static Vector<double> AxisInertiaVector(double[] eigenValues, int cols)
    {

        Vector<double> vectorInercia = Vector<double>.Build.Dense(eigenValues.Length);

        double value = 100 / cols;

        for (int i = 0; i < vectorInercia.Count; i++)
        {
            vectorInercia[i] = value * eigenValues[i];
        }

        return vectorInercia;

    }
}

/// <summary>
/// Struct que permite inicializar los puntos que se 
/// utilizaran para mostrar las graficas de ACP.
/// </summary>
public struct Point(double X, double Y)
{
    public double X = X;
    public double Y = Y;
    public string? PointName;

    public override string ToString()
    {
        return PointName + " at (" + X + ", " + Y + ")";
    }

}